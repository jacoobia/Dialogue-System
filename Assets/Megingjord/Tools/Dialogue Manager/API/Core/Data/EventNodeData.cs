using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    /// <summary>
    /// The data container for an event node
    /// </summary>
    [Serializable]
    public sealed class EventNodeData : DialogueNodeData {
        public string eventName;
    }
}