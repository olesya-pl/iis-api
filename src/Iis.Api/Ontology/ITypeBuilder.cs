﻿using System;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public interface ITypeBuilder
    {
        ITypeBuilder WithName(string name);
        ITypeBuilder WithTitle(string name);
        ITypeBuilder WithMeta(ISchemaMeta meta);
        ITypeBuilder Is(string name);
        ITypeBuilder Is(Action<ITypeBuilder> buildAction);
        ITypeBuilder HasRequired(string targetName);
        ITypeBuilder HasOptional(string targetName);
        ITypeBuilder HasMultiple(string targetName);
        ITypeBuilder HasRequired(Action<IRelationBuilder> relationDescriptor);
        ITypeBuilder HasOptional(Action<IRelationBuilder> relationDescriptor);
        ITypeBuilder HasMultiple(Action<IRelationBuilder> relationDescriptor);
        ITypeBuilder IsAbstraction();
        ITypeBuilder IsEntity();
        IAttributeBuilder IsAttribute();
        INodeTypeLinked Build();
    }

    public interface IAttributeBuilder : ITypeBuilder
    {
        IAttributeBuilder HasValueOf(ScalarType name);
    }

    public interface IRelationBuilder
    {
        IRelationBuilder WithName(string name);
        IRelationBuilder WithTitle(string title);
        IRelationBuilder WithMeta(ISchemaMeta meta);
        IRelationBuilder WithMeta<T>(Action<T> descriptor) where T : ISchemaMeta, new();
        IRelationBuilder Target(string targetName);
    }
}
