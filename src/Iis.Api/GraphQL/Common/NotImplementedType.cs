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

        public override Type RuntimeType => typeof(object);

        public override bool IsInstanceOfType(IValueNode valueSyntax)
        {
            throw new NotImplementedException();
        }

        public override object ParseLiteral(IValueNode valueSyntax)
        {
            throw new NotImplementedException();
        }

        public override IValueNode ParseResult(object resultValue)
        {
            throw new NotImplementedException();
        }

        public override IValueNode ParseValue(object runtimeValue)
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

        public override bool TrySerialize(object value, out object serialized)
        {
            throw new NotImplementedException();
        }
    }
}
