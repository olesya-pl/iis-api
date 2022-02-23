﻿using System;
using HotChocolate.Language;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.Entities.ObjectTypes
{
    public class PredictableDateType : ScalarType
    {
        private const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        
        public PredictableDateType() : base("PredictableDateType")
        {
        }

        public override Type RuntimeType { get; } = typeof(DateTime);

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

            if (value is DateTime s)
            {
                return new StringValueNode(null, s.ToString(DateFormat), false);
            }

            throw new ArgumentException(
                "The specified value has to be a DateTime in order " +
                "to be parsed by the type.");
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

            if (value is DateTime s)
            {
                return s.ToString(DateFormat);
            }

            throw new ArgumentException(
                "The specified value cannot be serialized by the PreidictabledataTime.");
        }

        public override bool TryDeserialize(object serialized, out object value)
        {
            value = null;
            try
            {
                var stringified = serialized.ToString();
                var success = DateTime.TryParse(stringified, out var parsed);
                if (success)
                {
                    value = parsed;
                }
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
}
