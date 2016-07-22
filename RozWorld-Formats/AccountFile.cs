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
using System.Text;

namespace Oddmatics.RozWorld.Formats
{
    /// <summary>
    /// Represents a RozWorld user account file.
    /// 
    /// The format for account files is:
    /// [1 BYTE] Username length
    /// [+ BYTES] Username (UTF8 encoded)
    /// [1 BYTE] Display name length
    /// [+ BYTES] Display name (UTF8 encoded)
    /// [32 BYTES] Password hash (should be SHA-256'd)
    /// [4 BYTES] Creation IPAddress
    /// [4 BYTES] Last login IPAddress
    /// [8 BYTES] Creation DateTime in ticks.
    /// </summary>
    public class AccountFile
    {
        /// <summary>
        /// Gets the creation date of this account.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets the IPAddress that created this account.
        /// </summary>
        public IPAddress CreationIP { get; private set; }

        /// <summary>
        /// The display name of this account.
        /// </summary>
        private string DisplayName;

        /// <summary>
        /// Gets the last logged in IPAddress of this account.
        /// </summary>
        public IPAddress LastLogInIP
        {
            get { return _LastLogInIP; }
            set { _LastLogInIP = value; Save(); }
        }
        private IPAddress _LastLogInIP;

        /// <summary>
        /// Gets the password hash of this account.
        /// </summary>
        public byte[] PasswordHash
        {
            get { return _PasswordHash; }
            set { if (value.Length == 32) _PasswordHash = value; Save(); }
        }
        private byte[] _PasswordHash;

        /// <summary>
        /// Gets the source filename of this account.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Gets the username of this account.
        /// </summary>
        public string Username { get; private set; }


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
            _PasswordHash = data.GetRange(currentIndex, 32).ToArray();
            currentIndex += 32;
            CreationIP = ByteParse.NextIPv4Address(data, ref currentIndex);
            _LastLogInIP = ByteParse.NextIPv4Address(data, ref currentIndex);
            CreationDate = new DateTime(ByteParse.NextLong(data, ref currentIndex));

            Source = filename;
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

            try
            {
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
                    fileData.AddRange(creatorAddressBytes);
                    fileData.AddRange(creatorAddressBytes); // Creator is also last-login initially
                    fileData.AddRange(DateTime.Now.Ticks.GetBytes());

                    FileSystem.PutBinaryFile(savePath, fileData.ToArray());

                    createdFile = new AccountFile(savePath);
                }
            }
            catch { }

            return createdFile;
        }


        /// <summary>
        /// Gets the display name safely.
        /// </summary>
        /// <returns>The display name of this account.</returns>
        public string GetDisplayName()
        {
            return DisplayName;
        }

        /// <summary>
        /// Updates the on-disk file of this AccountFile.
        /// </summary>
        private void Save()
        {
            var fileData = new List<byte>();

            fileData.AddRange(Username.GetBytesByLength(1, Encoding.UTF8));
            fileData.AddRange(DisplayName.GetBytesByLength(1, Encoding.UTF8));
            fileData.AddRange(PasswordHash);
            fileData.AddRange(CreationIP.GetAddressBytes());
            fileData.AddRange(LastLogInIP.GetAddressBytes()); // Creator is also last-login initially
            fileData.AddRange(DateTime.Now.Ticks.GetBytes());

            FileSystem.PutBinaryFile(Source, fileData.ToArray());
        }

        /// <summary>
        /// Sets the display name safely.
        /// </summary>
        /// <param name="name">The new display name.</param>
        /// <returns>True if the display name was set.</returns>
        public bool SetDisplayName(string name)
        {
            // Check the new display name is available
            string directory = Path.GetDirectoryName(Source);

            if (Directory.GetFiles(directory, "*." + name.ToLower() + ".acc").Length == 0)
            {
                // Name available - delete old file
                if (File.Exists(Source))
                    File.Delete(Source);

                DisplayName = name;
                Source = directory + "\\" + Username.ToLower() + "." + DisplayName.ToLower() + ".acc";
                Save(); // Update the file

                return true;
            }

            return false;
        }
    }
}
