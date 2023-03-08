using System;
using System.Collections.Generic;
using Megingjord.Tools.Dialogue_Manager.API.Core.Properties;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    /// <summary>
    /// The data container class that holds all of the information
    /// for a dialogue object
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Lonely Dialogue/Dialogue", order = 0)]
    public class DialogueData : ScriptableObject {

        [HideInInspector] public string entryGuid;
        [HideInInspector] public Vector2 entryNodePosition;
        [HideInInspector] public List<BasicNodeData> basicNodeData = new();
        [HideInInspector] public List<ChoiceNodeData> choiceNodeData = new();
        [HideInInspector] public List<EventNodeData> eventNodeData = new();
        [HideInInspector] public List<FocusNodeData> focusNodeData = new();
        [HideInInspector] public List<ConditionNodeData> conditionNodeData = new();
        [HideInInspector] public List<ExitNodeData> exitNodeData = new();
        [HideInInspector] public List<NodeLinkData> linkData = new();
        [HideInInspector] public bool firstTimeOpen = true;

        [HideInInspector] public List<IntProperty> intProperties = new();
        [HideInInspector] public List<StringProperty> stringProperties = new();
        [HideInInspector] public List<BoolProperty> boolProperties = new();

        /// <summary>
        /// Gets all the dialogue nodes as their superclass 
        /// </summary>
        /// <returns></returns>
        public List<DialogueNodeData> GetAllNodes() {
            var nodes = new List<DialogueNodeData> {
                new() {
                    guid = entryGuid,
                    position = entryNodePosition
                }
            };
            
            nodes.AddRange(basicNodeData);
            nodes.AddRange(focusNodeData);
            nodes.AddRange(choiceNodeData);
            nodes.AddRange(eventNodeData);
            nodes.AddRange(conditionNodeData);
            nodes.AddRange(exitNodeData);
            return nodes;
        }

        /// <summary>
        /// Gets all of the dialogue properties as their superclass
        /// </summary>
        /// <returns></returns>
        public List<DialogueProperty> GetAllProperties() {
            var properties = new List<DialogueProperty>();
            properties.AddRange(intProperties);
            properties.AddRange(stringProperties);
            properties.AddRange(boolProperties);
            return properties;
        }
        
    }
}