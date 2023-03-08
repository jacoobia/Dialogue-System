using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    /// <summary>
    /// The data container for a condition node
    /// </summary>
    [Serializable]
    public class ConditionNodeData : DialogueNodeData {
        public string variableName;
    }
}