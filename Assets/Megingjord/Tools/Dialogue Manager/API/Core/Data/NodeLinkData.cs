using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    /// <summary>
    /// The data container for a link between two nodes
    /// </summary>
    [Serializable]
    public sealed class NodeLinkData {
        public string outNodeGuid;
        public string inNodeGuid;
        public int portIndex;
    }
}