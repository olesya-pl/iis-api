using System;

namespace IIS.Core.Ontology
{
    class BuildException : Exception
    {
        public BuildException(string message) : base(message)
        {
        }
    }
}