namespace IIS.Core
{
    public interface IType : ISchemaNode
    {
        string Name { get; }
        Kind Kind { get; }
    }

    public enum Kind
    {
        Undefined,
        Attribute,
        Class,
        Union
    }
}
