using System.Collections.Generic;
using System.Linq;
using Megingjord.Tools.Dialogue_Manager.API.Core.Data;

namespace Megingjord.Tools.Dialogue_Manager.API.Core {
    /// <summary>
    /// A class responsible for parsing dialogue data
    /// Handles parsing of nodes & their data as well as
    /// handling any of the property replacers/conditions
    /// </summary>
    public class DialogueParser {

        private readonly DialogueData _dialogue;

        private DialogueNodeData _currentNode;

        private readonly Dictionary<string, string> _replacers = new();
        private readonly Dictionary<string, bool> _conditions = new();

        public DialogueParser(DialogueData dialogue) {
            _dialogue = dialogue;
            _currentNode = GetNodeByGuid(dialogue.entryGuid);
            BuildReplacers();
        }

        /// <summary>
        /// Gets a node by its guid
        /// </summary>
        /// <param name="guid">The guid to search for</param>
        /// <returns></returns>
        private DialogueNodeData GetNodeByGuid(string guid) {
            return _dialogue.GetAllNodes().Find(node => guid.Equals(node.guid));
        }
        
        /// <summary>
        /// Builds the dictionary of replacers to be used in
        /// dialogue text 
        /// </summary>
        private void BuildReplacers() {
            _replacers.Clear();
            var stringProps = _dialogue.stringProperties;
            var intProps = _dialogue.intProperties;
            var boolProps = _dialogue.boolProperties;

            foreach (var stringProp in stringProps) {
                var propName = "{" + stringProp.propertyName + "}";
                _replacers.Add(propName, stringProp.propertyValue);
            }
            
            foreach (var intProp in intProps) {
                var propName = "{" + intProp.propertyName + "}";
                _replacers.Add(propName, intProp.propertyValue.ToString());
            }
            
            foreach (var boolProp in boolProps) {
                var propName = "{" + boolProp.propertyName + "}";
                _replacers.Add(propName, boolProp.propertyValue.ToString());
                _conditions.Add(boolProp.propertyName, boolProp.propertyValue);
            }
        }
        
        /// <summary>
        /// Updates the value for a replacer, used if a property is
        /// updated at runtime
        /// </summary>
        /// <param name="key">The key/name of the property</param>
        /// <param name="value">The value to set the property/replacer to</param>
        public void UpdateReplacer(string key, object value) {
            var propName = "{" + key + "}";
            if (_replacers.ContainsKey(propName))
                _replacers[propName] = value.ToString();
        }

        /// <summary>
        /// Gets the text from a node and runs any replacers over it
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetText(DialogueNodeData data) {
            var text = data.text;
            foreach (var replacer in _replacers.Where(replacer => text.Contains(replacer.Key))) {
                text = text.Replace(replacer.Key, replacer.Value);
            }
            return text;
        }

        /// <summary>
        /// Checks a condition and returns the port index for the next node
        /// </summary>
        /// <param name="conditionVariable">The name of the property to check</param>
        /// <returns></returns>
        public int CheckCondition(string conditionVariable) {
            if (_conditions.ContainsKey(conditionVariable)) {
                return _conditions[conditionVariable] ? 0 : 1;
            }
            return -1;
        }

        /// <summary>
        /// Gets the next node in the chain, takes in an optional
        /// port name for those nodes that have multiple out ports
        ///
        /// Disabled the resharper suggestion to convert to ?: operator because
        /// this is more readable with a basic if statement
        /// </summary>
        /// <param name="portIndex">The optional port name</param>
        /// <returns></returns>
        public DialogueNodeData GetNext(int portIndex = 0) {
            // Find the links where the current node is the output
            var links = _dialogue.linkData.FindAll(link => _currentNode.guid.Equals(link.outNodeGuid));
            var nextNodeGuid = links.Find(link => portIndex == link.portIndex).inNodeGuid;
            _currentNode = GetNodeByGuid(nextNodeGuid);
            return _currentNode;
        }

    }
}