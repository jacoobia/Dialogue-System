using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    /// <summary>
    /// The data container for a focus node
    /// </summary>
    [Serializable]
    public sealed class FocusNodeData : DialogueNodeData {
        public int choice;
    }
}