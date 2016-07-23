/**
 * Oddmatics.RozWorld.Formats.PermissionGroupFile -- RozWorld Permission Group File
 *
 * This source-code is part of the file format I/O library for the RozWorld project by rozza of Oddmatics:
 * <<http://www.oddmatics.uk>>
 * <<http://roz.world>>
 * <<http://github.com/rozniak/RozWorld-Formats>>
 *
 * Sharing, editing and general licence term information can be found inside of the "LICENCE.MD" file that should be located in the root of this project's directory structure.
 */

using Newtonsoft.Json;
using Oddmatics.Util.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Oddmatics.RozWorld.Formats
{
    /// <summary>
    /// Represents a permission group JSON file.
    /// </summary>
    public class PermissionGroupFile
    {
        /// <summary>
        /// Gets the colour of the names in game chat for members of this group.
        /// </summary>
        public string Colour { get; set; }

        /// <summary>
        /// Gets whether this permission group is to be the default.
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// Gets the name of this permission group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the permissions granted by this group.
        /// </summary>
        public string[] Permissions { get; set; }

        /// <summary>
        /// Gets the game chat prefix to apply to members of this group.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets the game chat suffix to apply to members of this group.
        /// </summary>
        public string Suffix { get; set; }


        /// <summary>
        /// Creates a PermissionGroupFile from the specified file using JSON data in that file.
        /// </summary>
        /// <param name="filename">A string that contains the name of the file from which to create the PermissionGroupFile.</param>
        /// <returns>The PermissionGroupFile this method creates.</returns>
        public static PermissionGroupFile FromFile(string filename)
        {
            var data = FileSystem.GetTextFile(filename);
            string dataAsString = String.Empty;

            foreach (string line in data)
            {
                dataAsString += line + "\n";
            }

            return JsonConvert.DeserializeObject<PermissionGroupFile>(dataAsString);
        }


        /// <summary>
        /// Saves this PermissionGroupFile as JSON to the specified location.
        /// </summary>
        /// <param name="filename">The location to save to.</param>
        public void Save(string filename)
        {
            string data = JsonConvert.SerializeObject(this);
            FileSystem.PutTextFile(filename, new string[] { data });
        }
    }
}
