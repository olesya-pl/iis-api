using System.Collections.Generic;
using Iis.Interfaces.Ontology.Data;
namespace Iis.Interfaces.Ontology.Comparers
{
    public class NodeByIdComparer : IEqualityComparer<INode>
    {
        private static readonly NodeByIdComparer _comparer = new NodeByIdComparer();
        public static NodeByIdComparer Instance => _comparer;
        public bool Equals(INode x, INode y) => x?.Id == y?.Id;
        public int GetHashCode(INode obj) => obj.Id.GetHashCode();
    }
}