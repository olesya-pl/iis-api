using ProtoBuf;

namespace IIS.Search.Schema
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ComplexType))]
    [ProtoInclude(101, typeof(Attribute))]
    public abstract class Type
    {
        [ProtoMember(1)]
        public string Name { get; set; }
    }
}
