using System;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    /// <summary>
    /// The base data container for the dialogue nodes
    /// </summary>
    [Serializable]
    public class DialogueNodeData {
        public string guid;
        public string text;
        public Vector2 position;
    }
}