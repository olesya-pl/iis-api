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
        ITypeBuilder Is(Action<ITypeBuilder> buildAction);

        ITypeBuilder HasRequired(string targetName, string relationName = null, JObject meta = null, string title = null);
        ITypeBuilder HasRequired(Action<ITypeBuilder> buildAction);

        ITypeBuilder HasOptional(string targetName, string relationName = null, JObject meta = null, string title = null);
        ITypeBuilder HasOptional(Action<ITypeBuilder> buildAction);

        ITypeBuilder HasMultiple(string targetName, string relationName = null, JObject meta = null, string title = null);
        ITypeBuilder HasMultiple(Action<ITypeBuilder> buildAction);

        ITypeBuilder IsAbstraction();
        ITypeBuilder IsEntity();
        IRelationBuilder IsRelation();
        IAttributeBuilder IsAttribute();

        Type Build();
    }

    public interface IAttributeBuilder : ITypeBuilder
    {
        IAttributeBuilder HasValueOf(ScalarType name);
    }

    public interface IRelationBuilder : ITypeBuilder
    {
        ITypeBuilder To(Action<ITypeBuilder> buildAction);
        ITypeBuilder To(string name);
    }
}
