using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;

namespace IIS.Core.Ontology
{
    // TODO: remove this file together with Legacy folder when all applications migrated to .NET Core
    public class OntologyBuilder : IAttributeBuilder, ITypeBuilder
    {
        // TODO: business logic, including name checking should be moved into respective domain classes
        private static readonly Regex NameRegex = new Regex("^[_a-zA-Z][_a-zA-Z0-9]*$");

        private enum Kind : byte { Attribute, Entity, Abstraction }
        internal struct Relation
        {
            public string TargetName;
            public string RelationName;
            public EmbeddingOptions EmbeddingOptions;
            public IMeta Meta;
            public string Title;
        }
        private INodeTypeModel _builtType;
        private event EventHandler<EventArgs> TypeBuilt;
        private bool _isBuilding;

        public string Name => _name;
        private string _name;
        private string _title;
        private IMeta _meta;
        private List<string> _parents = new List<string>();
        private List<Action<ITypeBuilder>> _parentBuilders = new List<Action<ITypeBuilder>>();
        private List<Relation> _childNodes = new List<Relation>();
        private Kind _kind;
        private ScalarType _scalarType;

        private readonly Dictionary<string, OntologyBuilder> Builders;

        private OntologyBuilder GetBuilder(string name)
        {
            if (!Builders.TryGetValue(name, out var targetBuilder))
                throw new BuildException($"There is no type registered with code '{name}' while trying to build type '{_name}'");
            return targetBuilder;
        }

        public OntologyBuilder(Dictionary<string, OntologyBuilder> builders)
        {
            Builders = builders;
        }

        // Type
        public ITypeBuilder WithName(string name)
        {
            _name = name;
            if (Builders.ContainsKey(name))
                throw new BuildException($"Builder with name '{name}' is already created");
            Builders.Add(name, this);
            return this;
        }

        public ITypeBuilder WithTitle(string name)
        {
            _title = name;
            return this;
        }

        public ITypeBuilder WithMeta(IMeta meta)
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
            _parentBuilders.Add(buildAction);
            return this;
        }

        private ITypeBuilder HasRelation(EmbeddingOptions options, string targetName, string relationName = null, IMeta meta = null, string title = null)
        {
            _childNodes.Add(new Relation
            {
                TargetName = targetName,
                RelationName = relationName,
                EmbeddingOptions = options,
                Meta = meta,
                Title = title
            });
            return this;
        }

        private ITypeBuilder HasRelation(EmbeddingOptions options, Action<IRelationBuilder> relationDescriptor)
        {
            var builder = new RelationBuilder();
            builder.WithOptions(options);
            relationDescriptor(builder);
            _childNodes.Add(builder.Build());
            return this;
        }

        public ITypeBuilder HasRequired(string targetName) => HasRelation(EmbeddingOptions.Required, targetName);

        public ITypeBuilder HasOptional(string targetName) => HasRelation(EmbeddingOptions.Optional, targetName);

        public ITypeBuilder HasMultiple(string targetName) => HasRelation(EmbeddingOptions.Multiple, targetName);

        public ITypeBuilder HasRequired(Action<IRelationBuilder> relationDescriptor) => HasRelation(EmbeddingOptions.Required, relationDescriptor);

        public ITypeBuilder HasOptional(Action<IRelationBuilder> relationDescriptor) => HasRelation(EmbeddingOptions.Optional, relationDescriptor);

        public ITypeBuilder HasMultiple(Action<IRelationBuilder> relationDescriptor) => HasRelation(EmbeddingOptions.Multiple, relationDescriptor);


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

        public INodeTypeModel Build()
        {
            if (_builtType != null) return _builtType;

            _isBuilding = true;
            if (_name == null)
                throw new BuildException("Cannot build type without name");
            if (!NameRegex.IsMatch(_name))
                throw new BuildException($"Type name '{_name}' is not valid");

            var type = default(INodeTypeModel);
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
            else throw new BuildException($"Type '{_name}' kind was not specified");

            type.Title = _title ?? _name;
            type.Meta = _meta;
            // todo: make configurable
            var now = DateTime.UtcNow;
            type.CreatedAt = now;
            type.UpdatedAt = now;
            _builtType = type;

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
                var targetType = GetBuilder(parent).Build();
                var inheritance = new InheritanceRelationType(Guid.NewGuid()) { CreatedAt = now, UpdatedAt = now };
                inheritance.AddType(targetType);
                type.AddType(inheritance);
            }

            var createdProperties = new HashSet<string>();
            void buildRelation(Relation child, OntologyBuilder targetTypeBuilder)
            {
                var relationName = child.RelationName ?? child.TargetName;
                relationName = relationName.ToLowerCamelcase();
                var targetType = targetTypeBuilder.Build();
                if (!NameRegex.IsMatch(relationName))
                    throw new BuildException($"Relation name '{relationName}' is not valid");
                if (!createdProperties.Add(relationName))
                    throw new BuildException($"Type {_name} has duplicate property '{relationName}'");
                var embedding = new EmbeddingRelationType(Guid.NewGuid(), relationName, child.EmbeddingOptions)
                    { Meta = child.Meta, CreatedAt = now, UpdatedAt = now, Title = child.Title };
                embedding.AddType(targetType);
                type.AddType(embedding);
            }

            foreach (var child in _childNodes)
            {
                if (child.TargetName == null)
                    throw new BuildException($"Found relation with null target on builder '{_name}'");
                var targetBuilder = GetBuilder(child.TargetName);
                if (targetBuilder._isBuilding)
                {
                    targetBuilder.TypeBuilt += (sender, e) =>
                    {
                        buildRelation(child, (OntologyBuilder)sender);
                    };
                }
                else
                {
                    buildRelation(child, targetBuilder);
                }
            }
            _isBuilding = false;
            // TODO: get rid of this dirty shit...
            TypeBuilt?.Invoke(this, EventArgs.Empty);

            return type;
        }

        internal class RelationBuilder : IRelationBuilder
        {
            private Relation _relation;

            public RelationBuilder()
            {
                _relation = new Relation();
            }

            public IRelationBuilder WithName(string name)
            {
                _relation.RelationName = name;
                return this;
            }

            public IRelationBuilder WithTitle(string title)
            {
                _relation.Title = title;
                return this;
            }

            public IRelationBuilder WithMeta(RelationMetaBase meta)
            {
                _relation.Meta = meta;
                return this;
            }

            public IRelationBuilder WithMeta<T>(Action<T> descriptor) where T : RelationMetaBase, new()
            {
                if (_relation.Meta == null) _relation.Meta = new T();
                if (!(_relation.Meta is T typedMeta)) throw new ArgumentException($"Type of this relation meta was already set to {_relation.Meta.GetType()}");
                descriptor(typedMeta);
                return this;
            }

            public IRelationBuilder Target(string targetName)
            {
                _relation.TargetName = targetName;
                return this;
            }

            internal IRelationBuilder WithOptions(EmbeddingOptions options)
            {
                _relation.EmbeddingOptions = options;
                return this;
            }

            internal Relation Build() => _relation;
        }
    }
}
