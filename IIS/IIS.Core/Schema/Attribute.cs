using ProtoBuf;

namespace IIS.Core.Schema
{
    [ProtoContract]
    public class Attribute : Type
    {
        [ProtoMember(2)]
        public string DataType { get; set; }

        [ProtoMember(3)]
        public string ScalarType { get; set; }
    }
}
