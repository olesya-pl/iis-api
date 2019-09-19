using System;
using System.Collections.Generic;
using IIS.Core.Ontology.Meta;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public class OntologyBuilder : IAttributeBuilder, ITypeBuilder
    {
        private enum Kind { Attribute, Entity, Abstraction }
        private struct Relation
        {
            public string TargetName;
            public string RelationName;
            public EmbeddingOptions EmbeddingOptions;
            public JObject Meta;
            public string Title;
        }
        private Type _builtType;
        private event EventHandler<EventArgs> TypeBuilt;
        private bool _isBuilding;

        public string Name => _name;
        private string _name;
        private string _title;
        private JObject _meta;
        private List<string> _parents = new List<string>();
        private List<Action<ITypeBuilder>> _parentBuilders = new List<Action<ITypeBuilder>>();
        private List<Relation> _childNodes = new List<Relation>();
        private Kind _kind;
        private ScalarType _scalarType;

        private readonly Dictionary<string, OntologyBuilder> Builders;

        public OntologyBuilder(Dictionary<string, OntologyBuilder> builders)
        {
            Builders = builders;
        }

        // Type
        public ITypeBuilder WithName(string name)
        {
            _name = name;
            Builders.Add(name, this);
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

        public ITypeBuilder Is(Type type)
        {
            throw new NotImplementedException();
        }

        public ITypeBuilder Is(Action<ITypeBuilder> buildAction)
        {
            _parentBuilders.Add(buildAction);
            return this;
        }

        public ITypeBuilder HasRequired(string targetName, string relationName = null, JObject meta = null, string title = null)
        {
            _childNodes.Add(new Relation
            {
                TargetName = targetName,
                RelationName = relationName,
                EmbeddingOptions = EmbeddingOptions.Required,
                Meta = meta,
                Title = title
            });
            return this;
        }

        public ITypeBuilder HasRequired(Type type)
        {
            throw new NotImplementedException();
        }

        public ITypeBuilder HasOptional(string targetName, string relationName = null, JObject meta = null, string title = null)
        {
            _childNodes.Add(new Relation
            {
                TargetName = targetName,
                RelationName = relationName,
                EmbeddingOptions = EmbeddingOptions.Optional,
                Meta = meta,
                Title = title
            });
            return this;
        }

        public ITypeBuilder HasOptional(Type type)
        {
            throw new NotImplementedException();
        }

        public ITypeBuilder HasMultiple(string targetName, string relationName = null, JObject meta = null, string title = null)
        {
            _childNodes.Add(new Relation
            {
                TargetName = targetName,
                RelationName = relationName,
                EmbeddingOptions = EmbeddingOptions.Multiple,
                Meta = meta,
                Title = title
            });
            return this;
        }

        public ITypeBuilder HasMultiple(Type type)
        {
            throw new NotImplementedException();
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

        // Attr
        public IAttributeBuilder HasValueOf(ScalarType scalarType)
        {
            _scalarType = scalarType;
            return this;
        }

        public Type Build()
        {
            if (_builtType != null) return _builtType;

            _isBuilding = true;
            if (_name == null)
                throw new BuildException("Cannot build type without name");

            var type = default(Type);
            if (_kind == Kind.Attribute)
            {
                type = new AttributeType(Guid.NewGuid(), _name, _scalarType);
            }
            else if (_kind == Kind.Abstraction)
            {
                type = new EntityType(Guid.NewGuid(), _name, true);
            }
            else if (_kind == Kind.Entity)
            {
                type = new EntityType(Guid.NewGuid(), _name, false);
            }
            else throw new BuildException("Type kind was not specified");

            type.Title = _title ?? _name;
            type.Meta = _meta;
            // todo: make configurable
            var now = DateTime.UtcNow;
            type.CreatedAt = now;
            type.UpdatedAt = now;

            //foreach (var buildAction in _parentBuilders)
            //{
            //    var builder = new OntologyBuilder(_ontology);
            //    buildAction(builder);
            //    var targetType = builder.Build();
            //    _ontology.Add(targetType);
            //    var inheritance = new InheritanceRelationType(Guid.NewGuid());
            //    inheritance.AddType(targetType);
            //    type.AddType(inheritance);
            //}

            foreach (var parent in _parents)
            {
                var targetType = Builders[parent].Build();
                var inheritance = new InheritanceRelationType(Guid.NewGuid()) { CreatedAt = now, UpdatedAt = now };
                inheritance.AddType(targetType);
                type.AddType(inheritance);
            }

            foreach (var child in _childNodes)
            {
                if (!Builders.TryGetValue(child.TargetName, out var targetBuilder))
                    throw new BuildException($"There is no type registered with code '{child.TargetName}' while trying to build type '{_name}'");
                var relationName = child.RelationName ?? child.TargetName;
                relationName = relationName.ToLowerCamelcase();
                if (targetBuilder._isBuilding)
                {
                    targetBuilder.TypeBuilt += (sender, e) =>
                    {
                        var builder = (OntologyBuilder)sender;
                        var targetType = builder.Build();
                        var embedding = new EmbeddingRelationType(Guid.NewGuid(), relationName, child.EmbeddingOptions)
                        { Meta = child.Meta, CreatedAt = now, UpdatedAt = now, Title = child.Title };
                        embedding.AddType(targetType);
                        type.AddType(embedding);
                        // todo: remove quickfix
                        StubEntityOperations(embedding);
                        //end
                    };
                }
                else
                {
                    var targetType = targetBuilder.Build();
                    var embedding = new EmbeddingRelationType(Guid.NewGuid(), relationName, child.EmbeddingOptions)
                    { Meta = child.Meta, CreatedAt = now, UpdatedAt = now, Title = child.Title };
                    embedding.AddType(targetType);
                    type.AddType(embedding);
                    // todo: remove quickfix
                    StubEntityOperations(embedding);
                    //end
                }
            }
            _isBuilding = false;
            _builtType = type;
            TypeBuilt?.Invoke(this, EventArgs.Empty);

            return type;
        }

        private void StubEntityOperations(EmbeddingRelationType embedding)
        {
            if (embedding.IsEntityType && embedding.Meta == null)
                embedding.Meta = new EntityRelationMeta
                    {
                        AcceptsEntityOperations = new[] {EntityOperation.Create, EntityOperation.Update, EntityOperation.Delete}
                    }
                    .Serialize();
        }
    }
}
