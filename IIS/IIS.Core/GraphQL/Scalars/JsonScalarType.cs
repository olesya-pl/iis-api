using System;
using System.Collections.Generic;
using HotChocolate.Language;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Scalars
{
    public class JsonScalarType : HotChocolate.Types.ScalarType
    {
        public JsonScalarType() : base("JsonScalar")
        {
        }

        protected JsonScalarType(string name) : base(name)
        {
        }

        public override Type ClrType => typeof(object);

        public override bool IsInstanceOfType(IValueNode literal)
        {
            if (literal == null) throw new ArgumentNullException(nameof(literal));
            return literal is NullValueNode || literal is ObjectValueNode;
        }

        public override object ParseLiteral(IValueNode literal)
        {
            if (literal is NullValueNode) // NullValueNode is present after TryDeserialize call
                return null;
            throw new NotImplementedException();
        }

        public override IValueNode ParseValue(object value)
        {
            throw new NotImplementedException();
        }

        public override object Serialize(object value)
        {
            if (!(value is JObject jo))
                throw new ArgumentException(nameof(value));
//                return JObject.FromObject(value);
            return jo;
        }

        public override bool TryDeserialize(object serialized, out object value)
        {
            if (serialized is Dictionary<string, object>)
            {
                value = JObject.FromObject(serialized);
                return true;
            }

            if (serialized is JObject)
            {
                value = serialized;
                return true;
            }

            value = null;
            return false;
        }
    }
}
