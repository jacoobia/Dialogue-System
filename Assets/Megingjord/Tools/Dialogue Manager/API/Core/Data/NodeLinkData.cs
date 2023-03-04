using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Data {
    [Serializable]
    public sealed class NodeLinkData {
        public string outNodeGuid;
        public string inNodeGuid;
        public int portIndex;
    }
}