using System;
using System.Collections.Generic;
using Megingjord.Shared.Editor.IO;

namespace Megingjord.Tools.Editor.Prefab_History {
    [Serializable]
    public class PrefabHistoryData : ISaveData {

        public int totalPrefabs;
        public bool unpack;
        public bool selectOnPing;
        public List<PrefabInstance> prefabCache = new();
        
    }
}