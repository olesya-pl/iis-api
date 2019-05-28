using System;

namespace IIS.Core
{
    [Serializable]
    public class ConstraintNotFoundException : Exception
    {
        public ConstraintNotFoundException(string owner, string constraint)
            : base($"Type {owner} does not have constraint {constraint}.") { }

        public ConstraintNotFoundException(string owner, string constraint, Exception inner)
            : base($"Type {owner} does not have constraint {constraint}.", inner) { }
    }

    [Serializable]
    public class InvalidInheritanceException : Exception
    {
        public InvalidInheritanceException(string child, string parent)
            : base($"Inheritance violation between child {child} and parent {parent}. " +
                "Only one-tier inheritance from abstract type is supported.")
        { }

        public InvalidInheritanceException(string child, string parent, Exception inner)
            : base($"Inheritance violation between child {child} and parent {parent}. " +
                "Only one-tier inheritance from abstract type is supported.", inner)
        { }
    }

    [Serializable]
    public class ArrayUnsupportedException : Exception
    {
        public ArrayUnsupportedException(string owner, string constraint)
            : base($"Constraint {constraint} of type {owner} does not support array.") { }

        public ArrayUnsupportedException(string owner, string constraint, Exception inner)
            : base($"Constraint {constraint} of type {owner} does not support array.", inner) { }
    }

    [Serializable]
    public class SingleValueUnsupportedException : Exception
    {
        public SingleValueUnsupportedException(string constraint)
            : base($"Constraint {constraint} supports array only.") { }

        public SingleValueUnsupportedException(string constraint, Exception inner)
            : base($"Constraint {constraint} supports array only.", inner) { }
    }
}
