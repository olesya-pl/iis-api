using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace IIS.Search.Schema
{
    public enum Kind { Union, Interface, Class, ComplexAttribute }

    [ProtoContract]
    public class ComplexType : Type
    {
        [ProtoMember(2)]
        public Kind Kind { get; set; }
        [ProtoMember(3, AsReference = true)]
        public IEnumerable<ComplexType> Parents { get; set; } = new List<ComplexType>();
        [ProtoMember(4, AsReference = true)]
        public IEnumerable<Member> Members { get; set; } = new List<Member>();

        public IEnumerable<Member> AllMembers => Parents.SelectMany(p => p.Members).Concat(Members)
            .GroupBy(m => m.Name).Select(g => g.First()); // todo: finish
    }
}
