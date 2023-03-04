using System.Collections.Generic;
using System.Linq;
using Megingjord.Tools.Dialogue_Manager.API.Core.Data;

namespace Megingjord.Tools.Dialogue_Manager.API.Core {
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

        private DialogueNodeData GetNodeByGuid(string guid) {
            return _dialogue.GetAllNodes().Find(node => guid.Equals(node.guid));
        }
        
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
        
        public void UpdateReplacer(string key, object value) {
            var propName = "{" + key + "}";
            if (_replacers.ContainsKey(propName))
                _replacers[propName] = value.ToString();
        }

        public string GetText(DialogueNodeData data) {
            var text = data.text;
            foreach (var replacer in _replacers.Where(replacer => text.Contains(replacer.Key))) {
                text = text.Replace(replacer.Key, replacer.Value);
            }
            return text;
        }

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