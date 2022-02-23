using System;
using HotChocolate.Language;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public abstract class StringBasedScalarType : ScalarType
    {
        protected StringBasedScalarType(string typeName) : base(typeName)
        {
        }

        public override Type RuntimeType { get; } = typeof(string);

        // define which literals this type can be parsed from.
        public override bool IsInstanceOfType(IValueNode literal)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            return literal is StringValueNode
                || literal is NullValueNode;
        }

        // define how a literal is parsed to the native .NET type.
        public override object ParseLiteral(IValueNode valueSyntax, bool withDefaults = true)
        {
            if (valueSyntax == null)
            {
                throw new ArgumentNullException(nameof(valueSyntax));
            }

            if (valueSyntax is StringValueNode stringLiteral)
            {
                return stringLiteral.Value;
            }

            if (valueSyntax is NullValueNode)
            {
                return null;
            }

            throw new ArgumentException(
                "The string type can only parse string literals.",
                nameof(valueSyntax));
        }

        public override IValueNode ParseResult(object resultValue) => ParseValue(resultValue);

        // define how a native type is parsed into a literal,
        public override IValueNode ParseValue(object value)
        {
            if (value == null)
            {
                return new NullValueNode(null);
            }

            if (value is string s)
            {
                return new StringValueNode(null, s, false);
            }

            if (value is char c)
            {
                return new StringValueNode(null, c.ToString(), false);
            }

            throw new ArgumentException(
                "The specified value has to be a string or char in order " +
                "to be parsed by the string type.");
        }

        // define the result serialization. A valid output must be of the following .NET types:
        // System.String, System.Char, System.Int16, System.Int32, System.Int64,
        // System.Float, System.Double, System.Decimal and System.Boolean
        public override object Serialize(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string s)
            {
                return s;
            }

            if (value is char c)
            {
                return c;
            }

            throw new ArgumentException(
                "The specified value cannot be serialized by the StringType.");
        }

        public override bool TryDeserialize(object serialized, out object value)
        {
            value = null;
            try
            {
                value = serialized.ToString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool TrySerialize(object value, out object serialized)
        {
            serialized = null;
            try
            {
                serialized = Serialize(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class FloatRangeType : StringBasedScalarType
    {
        public FloatRangeType()
        : base("FloatRange")
        {
        }
    }

    public class IntegerRangeType : StringBasedScalarType
    {
        public IntegerRangeType()
        : base("IntegerRangeType")
        {
        }
    }
}
