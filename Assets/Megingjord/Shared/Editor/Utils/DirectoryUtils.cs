using System.Collections.Generic;
using System.Linq;
using Directory = System.IO.Directory;

namespace Megingjord.Shared.Editor.Utils {
    public static class DirectoryUtils {

        /// <summary>
        /// The base folder name for the tools
        /// </summary>
        private const string BaseFolder = "Megingjord";

        /// <summary>
        /// Gets the directory path to the root folder for Megingjord
        /// </summary>
        /// <returns></returns>
        public static string GetAssetDirectoryPath() {
            var root = Directory.GetCurrentDirectory();
            return GetDirectoryInDirectory(BaseFolder, root);
        }

        /// <summary>
        /// Gets a directory fom within a directory if it exists
        /// </summary>
        /// <param name="target">The target directory</param>
        /// <param name="dir">The directory to search</param>
        /// <returns></returns>
        private static string GetDirectoryInDirectory(string target, string dir) {
            var subDirectories = Directory.GetDirectories(dir);
            foreach (var directory in subDirectories) {
                if (directory.EndsWith(target))
                    return directory;
                var result = GetDirectoryInDirectory(target, directory);
                if (result != null) return result;
            }
            return null;
        }

        /// <summary>
        /// Collect prefabs in a directory and store their paths into a list 
        /// </summary>
        /// <param name="prefabs">The list to populate</param>
        /// <param name="dir">The directory to search</param>
        public static void CollectPrefabs(ref List<string> prefabs, string dir) {
            var subDirectories = Directory.GetDirectories(dir);
            var files = Directory.GetFiles(dir);
            
            prefabs.AddRange(files.Where(file => file.EndsWith(".prefab")));

            foreach (var directory in subDirectories) {
                CollectPrefabs(ref prefabs, directory);
            }
        }

    }
}