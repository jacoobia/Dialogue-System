using System.Collections.Generic;
using System.Linq;
using Directory = System.IO.Directory;

namespace Megingjord.Shared.Editor.Utils {
    public static class DirectoryUtils {

        private const string BaseFolder = "Megingjord";

        public static string GetAssetDirectoryPath() {
            var root = Directory.GetCurrentDirectory();
            return GetDirectoryInDirectory(BaseFolder, root);
        }

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