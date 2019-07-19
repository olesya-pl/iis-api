using System;
using HotChocolate;
using HotChocolate.Language;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Scalars
{
    public class JsonScalarType : HotChocolate.Types.ScalarType
    {
        public JsonScalarType() : base("JsonScalar"){}

        public override bool IsInstanceOfType(IValueNode literal)
        {
            if (literal == null) throw new ArgumentNullException(nameof(literal));
            return literal is NullValueNode || literal is ObjectValueNode;
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
            if (!(value is JObject jo)) throw new ArgumentException(nameof(value));
            return jo;
        }

        public override bool TryDeserialize(object serialized, out object value)
        {
            throw new NotImplementedException();
        }

        public override Type ClrType => typeof(JObject);
    }
}