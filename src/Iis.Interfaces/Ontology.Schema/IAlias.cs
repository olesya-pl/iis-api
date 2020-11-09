using Iis.Interfaces.Enums;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAlias
    {
        string DotName { get; }
        string Value { get; }
    }
}
