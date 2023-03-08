using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    /// <summary>
    /// The data container for a basic dialogue node
    /// </summary>
    [Serializable]
    public sealed class BasicNodeData : DialogueNodeData {
        public int choice;
    }
}