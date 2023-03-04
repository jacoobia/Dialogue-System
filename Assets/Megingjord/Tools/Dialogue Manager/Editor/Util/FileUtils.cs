using System.IO;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Util {
    public static class FileUtils {
        
        public static string IndexedFilename(string stub) {
            var ix = 0;
            string filename;
            do {
                ix++;
                filename = $"{stub}{ix}";
            } while (File.Exists($"Assets/{filename}.asset"));
            return filename;
        }
        
    }
}