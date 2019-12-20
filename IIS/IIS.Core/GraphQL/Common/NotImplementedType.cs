using System;
using HotChocolate.Language;

namespace IIS.Core.GraphQL.Common
{
    public class NotImplementedType : HotChocolate.Types.ScalarType
    {
        public NotImplementedType() : base("NotImplemented")
        {
            Description = "This field is not yet implemented. Don't even try querying it. Seriously.";
        }

        public override Type ClrType => typeof(object);

        public override bool IsInstanceOfType(IValueNode literal)
        {
            throw new NotImplementedException();
        }

        public override object ParseLiteral(IValueNode literal)
        {
            throw new NotImplementedException();
        }

        public override IValueNode ParseValue(object value)
        {
            throw new NotImplementedException();
        }

        public override object Serialize(object value)
        {
            throw new NotImplementedException();
        }

        public override bool TryDeserialize(object serialized, out object value)
        {
            throw new NotImplementedException();
        }

        //public override bool TrySerialize(object value, out object serialized)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
