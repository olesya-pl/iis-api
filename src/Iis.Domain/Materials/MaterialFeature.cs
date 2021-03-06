using System;

namespace Iis.Domain.Materials
{
    public class MaterialFeature
    {
        public Guid Id { get; }
        public string Relation { get; }
        public string Value { get; }
        public Node Node { get; set; }

        public MaterialFeature(Guid id, string relation, string value)
        {
            Id = id;
            Relation = relation;
            Value = value;
        }
    }
}
