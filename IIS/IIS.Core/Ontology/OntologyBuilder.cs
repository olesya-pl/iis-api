using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public class OntologyBuilder : IAttributeBuilder, IRelationBuilder, ITypeBuilder
    {
        private readonly Dictionary<string, OntologyBuilder> Builders;

        private enum Kind { Attribute, Entity, Abstraction, Relation }
        private Type _builtType;
        private event EventHandler<EventArgs> TypeBuilt;
        private bool _isBuilding;

        public string Name { get; private set; }
        private string _title;
        private JObject _meta;
        private List<string> _parents = new List<string>();
        private List<string> _children = new List<string>();
        private Kind _kind;
        private ScalarType _scalarType;
        private EmbeddingOptions _embeddingOptions;

        public OntologyBuilder(Dictionary<string, OntologyBuilder> builders)
        {
            Builders = builders;
        }

        // Type
        public ITypeBuilder WithName(string name)
        {
            Name = name;
            Builders[name] = this;
            return this;
        }

        public ITypeBuilder WithTitle(string name)
        {
            _title = name;
            return this;
        }

        public ITypeBuilder WithMeta(JObject meta)
        {
            _meta = meta;
            return this;
        }

        public ITypeBuilder Is(string name)
        {
            _parents.Add(name);
            return this;
        }

        public ITypeBuilder Is(Action<ITypeBuilder> buildAction)
        {
            var builder = new OntologyBuilder(Builders);
            buildAction(builder);
            Builders[builder.Name] = builder;
            _parents.Add(builder.Name);
            return this;
        }

        public ITypeBuilder To(Action<ITypeBuilder> buildAction)
        {
            var builder = new OntologyBuilder(Builders);
            buildAction(builder);
            Builders[builder.Name] = builder;
            _children.Add(builder.Name);
            return this;
        }

        public ITypeBuilder To(string name)
        {
            _children.Add(name);
            return this;
        }

        public ITypeBuilder HasRequired(Action<ITypeBuilder> buildAction)
        {
            return Has(EmbeddingOptions.Required, buildAction);
        }

        public ITypeBuilder HasOptional(Action<ITypeBuilder> buildAction)
        {
            return Has(EmbeddingOptions.Optional, buildAction);
        }

        public ITypeBuilder HasMultiple(Action<ITypeBuilder> buildAction)
        {
            return Has(EmbeddingOptions.Multiple, buildAction);
        }

        private ITypeBuilder Has(EmbeddingOptions embeddingOptions, Action<ITypeBuilder> buildAction)
        {
            var builder = new OntologyBuilder(Builders);
            buildAction(builder);
            builder._embeddingOptions = embeddingOptions;
            Builders[builder.Name] = builder;
            _children.Add(builder.Name);
            return this;
        }

        [Obsolete]
        public ITypeBuilder HasRequired(string targetName, string relationName = null, JObject meta = null, string title = null)
        {
            return Has(EmbeddingOptions.Required, b => b
                .WithName(relationName)
                .WithTitle(title)
                .WithMeta(meta)
                .IsRelation()
                .To(targetName)
            );
        }

        [Obsolete]
        public ITypeBuilder HasOptional(string targetName, string relationName = null, JObject meta = null, string title = null)
        {
            return Has(EmbeddingOptions.Optional, b => b
                .WithName(relationName)
                .WithTitle(title)
                .WithMeta(meta)
                .IsRelation()
                .To(targetName)
            );
        }

        [Obsolete]
        public ITypeBuilder HasMultiple(string targetName, string relationName = null, JObject meta = null, string title = null)
        {
            return Has(EmbeddingOptions.Multiple, b => b
                .WithName(relationName)
                .WithTitle(title)
                .WithMeta(meta)
                .IsRelation()
                .To(targetName)
            );
        }

        public ITypeBuilder IsAbstraction()
        {
            _kind = Kind.Abstraction;
            return this;
        }

        public ITypeBuilder IsEntity()
        {
            _kind = Kind.Entity;
            return this;
        }

        public IAttributeBuilder IsAttribute()
        {
            _kind = Kind.Attribute;
            return this;
        }

        public IRelationBuilder IsRelation()
        {
            _kind = Kind.Relation;
            return this;
        }

        public IAttributeBuilder HasValueOf(ScalarType scalarType)
        {
            _scalarType = scalarType;
            return this;
        }
        
        public Type Build()
        {
            if (_builtType != null) return _builtType;

            _isBuilding = true;

            var type = default(Type);
            if (_kind == Kind.Attribute)
            {
                type = new AttributeType(Guid.NewGuid(), Name, _scalarType);
            }
            else if (_kind == Kind.Abstraction)
            {
                type = new EntityType(Guid.NewGuid(), Name, true);
            }
            else if (_kind == Kind.Entity)
            {
                type = new EntityType(Guid.NewGuid(), Name, false);
            }
            else if (_kind == Kind.Relation)
            {
                type = new EmbeddingRelationType(Guid.NewGuid(), Name, _embeddingOptions, false);
            }

            type.Title = _title;
            type.Meta = _meta;
            // todo: make configurable
            var now = DateTime.UtcNow;
            type.CreatedAt = now;
            type.UpdatedAt = now;

            foreach (var parent in _parents)
            {
                var targetType = Builders[parent].Build();
                var inheritance = new InheritanceRelationType(Guid.NewGuid()) { CreatedAt = now, UpdatedAt = now };
                inheritance.AddType(targetType);
                type.AddType(inheritance);
            }

            foreach (var child in _children)
            {
                var targetBuilder = Builders[child];
                if (targetBuilder._isBuilding)
                {
                    targetBuilder.TypeBuilt += (sender, e) =>
                    {
                        var builder = (OntologyBuilder)sender;
                        var childType = builder.Build();
                        type.AddType(childType);
                    };
                }
                else
                {
                    var childType = targetBuilder.Build();
                    type.AddType(childType);
                }

                
            }

            _isBuilding = false;
            _builtType = type;
            TypeBuilt?.Invoke(this, EventArgs.Empty);

            return type;
        }
    }
}
