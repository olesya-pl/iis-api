using System;
using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Mutations;
using IIS.Core.GraphQL.Scalars;
using OScalarType = IIS.Core.Ontology.ScalarType;
using HCScalarType = HotChocolate.Types.ScalarType;

namespace IIS.Core.GraphQL
{
    public interface IGraphQlTypeProvider
    {
        Dictionary<string, IOutputType> OntologyTypes { get; }
        Dictionary<OScalarType, HCScalarType> Scalars { get; }
        T GetType<T>() where T : IType, new();
    }

    public class GraphQlTypeProvider : IGraphQlTypeProvider
    {
        public Dictionary<Type, IType> CommonTypes { get; } = new Dictionary<Type, IType>();
        
        public Dictionary<string, IOutputType> OntologyTypes { get; } = new Dictionary<string, IOutputType>();

        public Dictionary<OScalarType, HCScalarType> Scalars { get; } = new Dictionary<OScalarType, HCScalarType>
        {
            [OScalarType.String] = new StringType(),
            [OScalarType.Integer] = new IntType(),
            [OScalarType.Boolean] = new BooleanType(),
            [OScalarType.Decimal] = new DecimalType(),
            [OScalarType.DateTime] = new DateTimeType(), // HotChocolate uses ISO-8601 by default
            [OScalarType.Geo] = new GeoJsonScalarType(),
//            [OScalarType.File] = null, // Implemented as a dirty hack in Extensions.AttributeType(), because File attribute is not scalar
        };

        protected void AddType<T>() where T : IType, new() => CommonTypes.Add(typeof(T), new T());

        public T GetType<T>() where T : IType, new()
        {
            if (!CommonTypes.ContainsKey(typeof(T)))
                AddType<T>();
            return (T) CommonTypes[typeof(T)];
        }
    }
}