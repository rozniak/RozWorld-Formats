/**
 * Oddmatics.RozWorld.Formats.PlayerPermissionFile -- RozWorld Player Permission File
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

namespace Oddmatics.RozWorld.Formats
{
    /// <summary>
    /// Represents a player permissions JSON file.
    /// </summary>
    public class PlayerPermissionFile
    {
        /// <summary>
        /// Gets the colour of the name in game chat for this player.
        /// </summary>
        public string Colour { get; set; }

        /// <summary>
        /// Gets the individually denied permissions for this player.
        /// </summary>
        public string[] Denied { get; set; }

        /// <summary>
        /// Gets the individually granted permissions for this player.
        /// </summary>
        public string[] Granted { get; set; }

        /// <summary>
        /// Gets the name of the group that this player belongs to.
        /// </summary>
        public string Group { get; set; }
        
        /// <summary>
        /// Gets the name of this player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the game chat prefix to apply to this player.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets the game chat suffix to apply to this player.
        /// </summary>
        public string Suffix { get; set; }


        /// <summary>
        /// Creates a PlayerPermissionFile from the specified file using JSON data in that file.
        /// </summary>
        /// <param name="filename">A string that contains the name of the file from which to create the PlayerPermissionFile.</param>
        /// <returns>The PlayerPermissionFile this method creates.</returns>
        public static PlayerPermissionFile FromFile(string filename)
        {
            var data = FileSystem.GetTextFile(filename);
            string dataAsString = String.Empty;

            foreach (string line in data)
            {
                dataAsString += line + "\n";
            }

            return JsonConvert.DeserializeObject<PlayerPermissionFile>(dataAsString);
        }


        /// <summary>
        /// Saves this PlayerPermissionFile as JSON to the specified location.
        /// </summary>
        /// <param name="filename">The location to save to.</param>
        public void Save(string filename)
        {
            string data = JsonConvert.SerializeObject(this);
            FileSystem.PutTextFile(filename, new string[] { data });
        }
    }
}
