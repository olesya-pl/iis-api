using System;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public interface ITypeBuilder
    {
        ITypeBuilder WithName(string name);
        ITypeBuilder WithTitle(string name);
        ITypeBuilder WithMeta(JObject meta);
        ITypeBuilder Is(string name);
        ITypeBuilder Is(Type type);
        ITypeBuilder Is(Action<ITypeBuilder> buildAction);
        ITypeBuilder HasRequired(string targetName, string relationName = null, JObject meta = null, string title = null);
        ITypeBuilder HasRequired(Type type);
        ITypeBuilder HasOptional(string targetName, string relationName = null, JObject meta = null, string title = null);
        ITypeBuilder HasOptional(Type type);
        ITypeBuilder HasMultiple(string targetName, string relationName = null, JObject meta = null, string title = null);
        ITypeBuilder HasMultiple(Type type);
        ITypeBuilder IsAbstraction();
        ITypeBuilder IsEntity();
        IAttributeBuilder IsAttribute();
        Type Build();
    }

    public interface IAttributeBuilder : ITypeBuilder
    {
        IAttributeBuilder HasValueOf(ScalarType name);
    }
}
