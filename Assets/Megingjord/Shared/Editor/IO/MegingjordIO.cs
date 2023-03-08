using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Megingjord.Shared.Editor.IO {
    /// <summary>
    /// The shared Megingjord IO utility class for loading and saving
    /// data used by the tools
    /// </summary>
    public static class MegingjordIO {
        
        private const string FileExtension = ".msf";

        /// <summary>
        /// Saves data to a binary data file within the save
        /// folder for a certain save.
        /// </summary>
        /// <param name="saveData">The data to save</param>
        /// <param name="root"></param>
        /// <typeparam name="T"></typeparam>
        public static void Save<T>(T saveData, string root) where T : ISaveData {
            var formatter = new BinaryFormatter();
            var path = string.Concat(root, "/", saveData.GetType().Name, FileExtension);
            var stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, saveData);
            stream.Close();
        }

        /// <summary>
        /// Loads & reads a binary data file from the save data
        /// directory, if one doesn't exist it creates a new one.
        /// Used for save game data
        /// </summary>
        /// <param name="root"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>(string root) where T : ISaveData {
            var path = string.Concat(root, "/", typeof(T).Name, FileExtension);
            if (!File.Exists(path)) return (T)Activator.CreateInstance(typeof(T));
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Open);
            var data = (T)formatter.Deserialize(stream);
            stream.Close();
            return data;
        }
        
    }
}