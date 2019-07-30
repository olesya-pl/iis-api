using System;
using System.Collections.Generic;
using System.Linq;
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
        ITypeBuilder HasRequired(string name, JObject meta);
        ITypeBuilder HasRequired(Type type);
        ITypeBuilder HasOptional(string name, JObject meta);
        ITypeBuilder HasOptional(Type type);
        ITypeBuilder HasMultiple(string name, JObject meta);
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

    public class OntologyBuilder : IAttributeBuilder, ITypeBuilder
    {
        private enum Kind { Attribute, Entity, Abstraction }
        private struct Relation
        {
            public string Name;
            public EmbeddingOptions EmbeddingOptions;
            public JObject Meta { get; set; }
        }
        private Type _builtType;
        public bool IsBuilt => _builtType != null;
        public bool IsBuilding { get; private set; }
        event EventHandler<EventArgs> TypeBuilt;

        public string Name => _name;
        private string _name;
        private string _title;
        private JObject _meta;
        private List<string> _parents = new List<string>();
        private List<Action<ITypeBuilder>> _parentBuilders = new List<Action<ITypeBuilder>>();
        private List<Relation> _childNodes = new List<Relation>();
        private Kind _kind;
        private ScalarType _scalarType;

        private static Dictionary<string, OntologyBuilder> Builders = new Dictionary<string, OntologyBuilder>();

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

        public ITypeBuilder HasRequired(string name, JObject meta)
        {
            _childNodes.Add(new Relation { Name = name, EmbeddingOptions = EmbeddingOptions.Required, Meta = meta });
            return this;
        }

        public ITypeBuilder HasRequired(Type type)
        {
            throw new NotImplementedException();
        }

        public ITypeBuilder HasOptional(string name, JObject meta)
        {
            _childNodes.Add(new Relation { Name = name, EmbeddingOptions = EmbeddingOptions.Optional, Meta = meta });
            return this;
        }

        public ITypeBuilder HasOptional(Type type)
        {
            throw new NotImplementedException();
        }

        public ITypeBuilder HasMultiple(string name, JObject meta)
        {
            _childNodes.Add(new Relation { Name = name, EmbeddingOptions = EmbeddingOptions.Multiple, Meta = meta });
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
            if (IsBuilt) return _builtType;

            IsBuilding = true;

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

            type.Title = _title;
            type.Meta = _meta;

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
                var inheritance = new InheritanceRelationType(Guid.NewGuid());
                inheritance.AddType(targetType);
                type.AddType(inheritance);
            }

            foreach (var child in _childNodes)
            {
                var targetBuilder = Builders[child.Name];

                if (targetBuilder.IsBuilt)
                {
                    var targetType = targetBuilder.Build();
                    var embedding = new EmbeddingRelationType(Guid.NewGuid(), child.Name, child.EmbeddingOptions)
                    { Meta = child.Meta };
                    embedding.AddType(targetType);
                    type.AddType(embedding);
                }
                else if (targetBuilder.IsBuilding)
                {
                    targetBuilder.TypeBuilt += (sender, e) =>
                    {
                        var builder = (OntologyBuilder)sender;
                        var targetType = builder.Build();
                        var embedding = new EmbeddingRelationType(Guid.NewGuid(), child.Name, child.EmbeddingOptions)
                        { Meta = child.Meta };
                        embedding.AddType(targetType);
                        type.AddType(embedding);
                    };
                }
            }
            IsBuilding = false;
            _builtType = type;
            TypeBuilt?.Invoke(this, EventArgs.Empty);

            return type;
        }
    }
}
