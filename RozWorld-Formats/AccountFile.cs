/**
 * Oddmatics.RozWorld.Formats.AccountFile -- RozWorld Account File
 *
 * This source-code is part of the file format I/O library for the RozWorld project by rozza of Oddmatics:
 * <<http://www.oddmatics.uk>>
 * <<http://roz.world>>
 * <<http://github.com/rozniak/RozWorld-Formats>>
 *
 * Sharing, editing and general licence term information can be found inside of the "LICENCE.MD" file that should be located in the root of this project's directory structure.
 */

using Oddmatics.Util.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Oddmatics.RozWorld.Formats
{
    /// <summary>
    /// (Deprecated) Represents a RozWorld user account file.
    /// </summary>
    /// <remarks>
    /// The format for account files is:
    /// [1 BYTE] File version
    /// [1 BYTE] Username length
    /// [+ BYTES] Username (UTF8 encoded)
    /// [1 BYTE] Display name length
    /// [+ BYTES] Display name (UTF8 encoded)
    /// [32 BYTES] Password hash (should be SHA-256'd)
    /// [1 BYTE] Creation IP version, Last login IP version.
    /// [4/16 BYTES] Creation IPAddress
    /// [4/16 BYTES] Last login IPAddress
    /// [8 BYTES] Creation DateTime in ticks.
    /// </remarks>
    public class AccountFile
    {
        /// <summary>
        /// The latest version of the account file file format that this library reads.
        /// </summary>
        public const byte FORMAT_VERSION = 1;


        /// <summary>
        /// Gets the creation date of this account.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets the IP address that created this account.
        /// </summary>
        public IPAddress CreationIP { get; private set; }

        /// <summary>
        /// Gets or sets the display name of this account.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// Gets or sets the last logged in IP address of this account.
        /// </summary>
        public IPAddress LastLoginIP;

        /// <summary>
        /// Gets or sets the password hash of this account.
        /// </summary>
        public byte[] PasswordHash;

        /// <summary>
        /// Gets or sets the username of this account.
        /// </summary>
        public string Username;


        /// <summary>
        /// Initialises a new instance of the AccountFile class with a filename to load from.
        /// </summary>
        /// <param name="filename">The filename of the account to load.</param>
        public AccountFile(string filename)
        {
            var data = new List<byte>(FileSystem.GetBinaryFile(filename));
            int currentIndex = 0;

            Username = ByteParse.NextStringByLength(data, ref currentIndex, 1, Encoding.UTF8);
            DisplayName = ByteParse.NextStringByLength(data, ref currentIndex, 1, Encoding.UTF8);
            PasswordHash = data.GetRange(currentIndex, 32).ToArray();
            currentIndex += 32;

            byte ipFlags = ByteParse.NextByte(data, ref currentIndex);
            bool creatorIPv6 = ((ipFlags & 0xF0) >> 4) == 6;
            bool lastLoginIPv6 = (ipFlags & 0x0F) == 6;

            CreationIP = creatorIPv6 ?
                ByteParse.NextIPv6Address(data, ref currentIndex) :
                ByteParse.NextIPv4Address(data, ref currentIndex);
            LastLoginIP = lastLoginIPv6 ?
                ByteParse.NextIPv6Address(data, ref currentIndex) :
                ByteParse.NextIPv4Address(data, ref currentIndex);

            CreationDate = new DateTime(ByteParse.NextLong(data, ref currentIndex));
        }


        /// <summary>
        /// Creates a new AccountFile instance and data on disk with the specified username, password hash and creation IPAddress.
        /// </summary>
        /// <param name="username">The username to use for this account.</param>
        /// <param name="passwordHash">The password hash to use for this account.</param>
        /// <param name="creationIP">The creator's IPAddress to use for this account.</param>
        /// <param name="directory">The directory path to save in.</param>
        /// <returns>The newly created file as an AccountFile instance if it was successful, null otherwise.</returns>
        public static AccountFile Create(string username, byte[] passwordHash, IPAddress creationIP, string directory)
        {
            byte[] creatorAddressBytes = creationIP.GetAddressBytes();
            AccountFile createdFile = null;
            var data = new List<byte>();
            string realName = username.ToLower();

            if (Directory.GetFiles(directory, realName + ".*.acc").Length == 0)
            {
                const byte maxAttempts = 4;
                byte attempts = 0;
                string finalDisplayName = String.Empty;
                string savePath = String.Empty;
                string underscores = String.Empty;

                while (Directory.GetFiles(directory, "*." + realName + underscores + ".acc")
                    .Length > 0)
                {
                    if (++attempts > maxAttempts ||
                        (realName + underscores).Length > 255)
                        return null;

                    underscores += "_";
                }

                finalDisplayName = username + underscores;
                savePath = directory + "\\" + realName + "." + realName +
                    underscores + ".acc";

                var fileData = new List<byte>();

                fileData.AddRange(username.GetBytesByLength(1, Encoding.UTF8));
                fileData.AddRange(finalDisplayName.GetBytesByLength(1, Encoding.UTF8));
                fileData.AddRange(passwordHash);

                // Add IP version flags
                if (creationIP.AddressFamily == AddressFamily.InterNetwork)
                    fileData.Add(0x44);
                else if (creationIP.AddressFamily == AddressFamily.InterNetworkV6)
                    fileData.Add(0x66);
                else
                    throw new ArgumentException("AccountFile.Create: creationIP must be either an IPv4 or IPv6 address.");

                fileData.AddRange(creatorAddressBytes);
                fileData.AddRange(creatorAddressBytes); // Creator is also last-login initially
                fileData.AddRange(DateTime.Now.Ticks.GetBytes());

                FileSystem.PutBinaryFile(savePath, fileData.ToArray());

                createdFile = new AccountFile(savePath);
            }

            return createdFile;
        }

        /// <summary>
        /// Updates the on-disk file of this AccountFile.
        /// </summary>
        public void Save(string filename)
        {
            var fileData = new List<byte>();
            byte ipFlags = 0;

            if (CreationIP.AddressFamily == AddressFamily.InterNetwork)
                ipFlags += 0x40;
            else if (CreationIP.AddressFamily == AddressFamily.InterNetworkV6)
                fileData.Add(0x60);
            else
                throw new ArgumentException("AccountFile.Save: CreationIP must be either an IPv4 or IPv6 address.");

            if (LastLoginIP.AddressFamily == AddressFamily.InterNetwork)
                fileData.Add(0x04);
            else if (LastLoginIP.AddressFamily == AddressFamily.InterNetworkV6)
                fileData.Add(0x06);
            else
                throw new ArgumentException("AccountFile.Save: CreationIP must be either an IPv4 or IPv6 address.");

            fileData.AddRange(Username.GetBytesByLength(1, Encoding.UTF8));
            fileData.AddRange(DisplayName.GetBytesByLength(1, Encoding.UTF8));
            fileData.AddRange(PasswordHash);
            fileData.AddRange(CreationIP.GetAddressBytes());
            fileData.AddRange(LastLoginIP.GetAddressBytes()); // Creator is also last-login initially
            fileData.AddRange(DateTime.Now.Ticks.GetBytes());

            FileSystem.PutBinaryFile(filename, fileData.ToArray());
        }
    }
}
