using System;
using UnityEngine.Rendering;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    [Serializable]
    public sealed class ChoiceNodeData : DialogueNodeData {

        public SerializedDictionary<int, string> choices = new();
        
    }
}