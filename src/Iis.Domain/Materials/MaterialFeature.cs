using Iis.DataModel.Materials;
using System;

namespace Iis.Domain.Materials
{
    public class MaterialFeature
    {
        public Guid Id { get; }
        public string Relation { get; }
        public string Value { get; }
        public Node Node { get; set; }
        public MaterialNodeLinkType NodeLinkType { get; set; }
    }
}
