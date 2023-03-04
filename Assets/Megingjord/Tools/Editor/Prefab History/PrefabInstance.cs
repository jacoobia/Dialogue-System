using System;
using Megingjord.Shared.Editor.IO;

namespace Megingjord.Tools.Editor.Prefab_History {
    [Serializable]
    public class PrefabInstance : ISaveData {

        public string name;
        public string path;
        public bool pinned;
        public DateTime added;
    }
}