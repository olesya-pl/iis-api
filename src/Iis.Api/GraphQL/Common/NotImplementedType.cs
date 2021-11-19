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

        public override IValueNode ParseResult(object resultValue)
            => ParseValue(resultValue);

        public override bool IsInstanceOfType(IValueNode valueSyntax)
        {
            throw new NotImplementedException();
        }

        public override object ParseLiteral(IValueNode valueSyntax)
        {
            throw new NotImplementedException();
        }

        public override IValueNode ParseValue(object runtimeValue)
        {
            throw new NotImplementedException();
        }

        public override object Serialize(object runtimeValue)
        {
            throw new NotImplementedException();
        }

        public override bool TryDeserialize(object resultValue, out object runtimeValue)
        {
            throw new NotImplementedException();
        }

        public override bool TrySerialize(object runtimeValue, out object resultValue)
        {
            throw new NotImplementedException();
        }
    }
}
