using System.Collections.Generic;
using ProtoBuf;

namespace IIS.Core.Schema
{
    public enum Kind { Union, Interface, Class, ComplexAttribute }

    [ProtoContract]
    public class ComplexType : Type
    {
        [ProtoMember(2)]
        public Kind Kind { get; set; }

        [ProtoMember(3, AsReference = true)]
        public ICollection<ComplexType> Parents { get; set; } = new List<ComplexType>();

        [ProtoMember(4, AsReference = true)]
        public ICollection<Member> Members { get; set; } = new List<Member>();

        public Attribute AddAttribute(string name, ScalarType type, bool isRequired = false, bool isArray = false)
        {
            var attribute = new Attribute { Name = name, ScalarType = (string)type };

            Members.Add(new Member { Name = name, IsRequired = isRequired, IsArray = isArray, Target = attribute });

            return attribute;
        }

        public void AddComplexType(string name, ComplexType type, bool isRequired = false, bool isArray = false)
        {
            Members.Add(new Member { Name = name, Target = type, IsRequired = isRequired, IsArray = isArray });
        }
    }
}
