using System;

namespace IIS
{
    public class ObjectEnum : IEquatable<ObjectEnum>
    {
        protected readonly string _value;

        protected ObjectEnum(string value) { _value = value; }

        public bool Equals(ObjectEnum other) => _value.Equals(other?._value);

        public override string ToString() => _value;

        public override bool Equals(object obj) => Equals(obj as ScalarType);

        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(ObjectEnum a, ObjectEnum b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;

            return a.Equals(b);
        }

        public static bool operator !=(ObjectEnum a, ObjectEnum b) => !(a == b);

        public static explicit operator ObjectEnum(string value) => new ObjectEnum(value);

        public static explicit operator string(ObjectEnum value) => value._value;
    }
}
