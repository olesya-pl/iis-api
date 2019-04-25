using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;
using IIS.Storage.EntityFramework.Resolvers;

namespace IIS.Storage.EntityFramework
{
    // todo: make real builder to be able to cover it with unit-tests
    public class GraphTypeBuilder
    {
        private static readonly Dictionary<string, IComplexGraphType> ComplexTypes = new Dictionary<string, IComplexGraphType>();
        private static readonly Dictionary<string, ScalarGraphType> ScalarTypes = new Dictionary<string, ScalarGraphType>
        {
            ["string"] = new StringGraphType(),
            ["int"] = new IntGraphType(),
            ["decimal"] = new DecimalGraphType(),
            ["date"] = new DateGraphType(),
            ["boolean"] = new BooleanGraphType(),
            ["geo"] = new StringGraphType(), // todo: custom scalar
            ["file"] = new StringGraphType(),// todo: custom object
            ["json"] = new StringGraphType()
        };

        static GraphTypeBuilder()
        {
            foreach (var type in ScalarTypes.Keys)
            {
                var value = new ObjectGraphType { Name = type.Camelize() + "Value" };
                value.AddField(new FieldType { Name = "Id", ResolvedType = new NonNullGraphType(ScalarTypes["int"]) });
                value.AddField(new FieldType { Name = "Value", ResolvedType = new NonNullGraphType(ScalarTypes[type]) });

                ComplexTypes[type] = value;
            }

            var relationType = new ObjectGraphType { Name = "Relation" };
            relationType.AddField(new FieldType { Name = nameof(ORelation.Id), ResolvedType = new NonNullGraphType(new IntGraphType()) });
            relationType.AddField(new FieldType { Name = nameof(ORelation.StartsAt), ResolvedType = new DateTimeGraphType() });
            relationType.AddField(new FieldType { Name = nameof(ORelation.EndsAt), ResolvedType = new DateTimeGraphType() });
            relationType.AddField(new FieldType { Name = nameof(ORelation.CreatedAt), ResolvedType = new NonNullGraphType(new DateTimeGraphType()) });
            relationType.AddField(new FieldType { Name = nameof(ORelation.IsInferred), ResolvedType = new NonNullGraphType(new BooleanGraphType()) });

            ComplexTypes[relationType.Name] = relationType;
        }

        private readonly ContourContext _context;
        private readonly OType _type;
        private IComplexGraphType _graphType;

        public GraphTypeBuilder(ContourContext context, OType type)
        {
            _context = context;
            _type = type;
        }

        public IComplexGraphType Build()
        {
            if (ComplexTypes.ContainsKey(_type.Code)) return ComplexTypes[_type.Code];

            NewGraphType(_type);
            EnsureInterface(_type);
            EnsureServiceAttributes();
            EnsureAttributes(_type);
            EnsureRelations(_type);
            EnsureServiceRelationField();

            return _graphType;
        }

        public void NewGraphType(OType type)
        {
            _graphType = type.IsAbstract
                ? new InterfaceGraphType { Name = type.Code } as IComplexGraphType
                : new ObjectGraphType { Name = type.Code } as IComplexGraphType;

            ComplexTypes[type.Code] = _graphType;
        }

        public void EnsureInterface(OType type)
        {
            if (type.Parent == null) return;

            var abstractType = (IInterfaceGraphType)new GraphTypeBuilder(_context, type.Parent).Build();
            var missingFields = abstractType.Fields.Where(field => !_graphType.HasField(field.Name));

            foreach (var field in missingFields) _graphType.AddField(field);

            var objectType = (IObjectGraphType)_graphType;
            objectType.AddResolvedInterface(abstractType);
            objectType.IsTypeOf = entity => ((OEntity)entity).Type.Code == type.Code;
        }

        public void EnsureAttributes(OType type)
        {
            foreach (var attr in type.AttributeRestrictions)
            {
                var name = attr.Attribute.Code;
                var field = _graphType.HasField(name) ? _graphType.GetField(name) : new FieldType { Name = name };

                field.ResolvedType = attr.IsMultiple 
                    ? new NonNullGraphType(new ListGraphType(new NonNullGraphType(ComplexTypes[attr.Attribute.Type])))
                    : (attr.IsRequired ? (IGraphType)new NonNullGraphType(ScalarTypes[attr.Attribute.Type]) : ScalarTypes[attr.Attribute.Type]);

                field.Resolver = new AttributeFieldResolver(attr.IsMultiple);

                if (!_graphType.HasField(name)) _graphType.AddField(field);
            }
        }

        public void EnsureRelations(OType type)
        {
            var relationGroups = type.ForwardRestrictions.GroupBy(r => r.RelationType.Code);

            foreach (var group in relationGroups)
            {
                if (_graphType.HasField(group.Key)) continue; // todo: override meta of parent type
                // todo: compare metadata instead
                if (group.Any(g => group.Any(ig => ig.IsMultiple != g.IsMultiple)) || !group.Any())
                    throw new Exception("Unsupported relation configuration.");

                var childType = default(IGraphType);
                var restriction = group.First();

                if (group.Count() > 1)
                {
                    // todo: camelize and nonNull for union parts
                    var unionType = new UnionGraphType { Name = $"{type.Code}{group.Key}RelationUnion" };
                    var possibleTypes = group.Select(item => (IObjectGraphType)new GraphTypeBuilder(_context, item.Target).Build());
                    foreach (var item in possibleTypes) unionType.AddPossibleType(item);
                    childType = unionType;
                }
                else if (group.Count() == 1) childType = new GraphTypeBuilder(_context, restriction.Target).Build();

                if (restriction.IsRequired && !restriction.IsMultiple) childType = new NonNullGraphType(childType);

                // todo: pluralize
                // todo: set IsMultiple in RelationType
                if (group.First().IsMultiple) childType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(childType)));

                var resolver = group.First().IsMultiple ? (IFieldResolver)new ListResolver(_context) : new EntityResolver(_context);

                _graphType.AddField(new FieldType { Name = group.Key, ResolvedType = childType, Resolver = resolver });
            }
        }

        public void EnsureServiceAttributes()
        {
            if (!_graphType.HasField(nameof(OType.Id)))
                _graphType.AddField(new FieldType { Name = nameof(OType.Id), ResolvedType = new NonNullGraphType(new IntGraphType()) });
        }

        public void EnsureServiceRelationField()
        {
            if (!_graphType.HasField("_relation"))
                _graphType.AddField(new FieldType
                {
                    Name = "_relation",
                    ResolvedType = new NonNullGraphType(ComplexTypes["Relation"]),
                    Resolver = new RelationResolver()
                });
        }
    }
}
