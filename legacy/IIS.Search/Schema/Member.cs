using ProtoBuf;

namespace IIS.Search.Schema
{
    public enum Membership { Type, Attribute }

    [ProtoContract]
    public class Member
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public bool IsArray { get; set; }
        [ProtoMember(3)]
        public bool IsRequired { get; set; }
        [ProtoMember(4, AsReference = true)]
        public Type Target { get; set; }

        public Membership Membership => Target is ComplexType ? Membership.Type : Membership.Attribute;
    }
}
