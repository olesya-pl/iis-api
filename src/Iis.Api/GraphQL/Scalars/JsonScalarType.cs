using System;
using System.Collections.Generic;
using HotChocolate.Language;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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

        public override Type RuntimeType => typeof(object);

        public override bool IsInstanceOfType(IValueNode valueSyntax)
        {
            if (valueSyntax == null) throw new ArgumentNullException(nameof(valueSyntax));
            return valueSyntax is NullValueNode || valueSyntax is ObjectValueNode;
        }

        public override IValueNode ParseResult(object resultValue)
            => ParseValue(resultValue);

        public override object ParseLiteral(IValueNode valueSyntax)
        {
            if (valueSyntax is NullValueNode) // NullValueNode is present after TryDeserialize call
                return null;
            throw new NotImplementedException();
        }

        public override IValueNode ParseValue(object runtimeValue)
        {
            throw new NotImplementedException();
        }

        public override object Serialize(object runtimeValue)
        {
            if (!(runtimeValue is JObject jo))
                throw new ArgumentException(nameof(runtimeValue));
            return jo;
        }

        public override bool TrySerialize(object runtimeValue, out object resultValue)
        {
            try
            {
                resultValue = JsonConvert.SerializeObject(runtimeValue);
                return true;
            }
            catch
            {
                resultValue = null;
                return false;
            }
        }

        public override bool TryDeserialize(object resultValue, out object runtimeValue)
        {
            if (resultValue is Dictionary<string, object>)
            {
                runtimeValue = JObject.FromObject(resultValue);
                return true;
            }

            if (resultValue is JObject)
            {
                runtimeValue = resultValue;
                return true;
            }

            runtimeValue = null;
            return false;
        }
    }
}
