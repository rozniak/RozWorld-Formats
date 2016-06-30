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
using System.IO;
using System.Net;

namespace Oddmatics.RozWorld.Formats
{
    /// <summary>
    /// Represents a RozWorld user account file.
    /// </summary>
    public class AccountFile
    {
        /// <summary>
        /// Gets the IPAddress that created this account.
        /// </summary>
        public IPAddress CreationIP { get; private set; }

        /// <summary>
        /// Gets the display name of this account.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the last logged in IPAddress of this account.
        /// </summary>
        public IPAddress LastLogInIP { get; private set; }

        /// <summary>
        /// Gets the password hash of this account.
        /// </summary>
        public byte[] PasswordHash { get; private set; }

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
            // Load here
        }

        /// <summary>
        /// Initialises a new instance of the AccountFile class with a username, password hash and creation IPAddress.
        /// </summary>
        /// <param name="username">The username to set for this account.</param>
        /// <param name="passwordHash">The password to set for this account.</param>
        /// <param name="creationIP">The creation IP of this account.</param>
        public AccountFile(string username, byte[] passwordHash, IPAddress creationIP)
        {
            CreationIP = creationIP;
            DisplayName = username;
            LastLogInIP = creationIP;
            PasswordHash = passwordHash;
            Username = username;
        }
    }
}
