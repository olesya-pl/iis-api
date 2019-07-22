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
        T GetType<T>() where T : IType;
        T GetType<T>(Type type) where T : IType;
    }

    public class GraphQlTypeProvider : IGraphQlTypeProvider
    {
        public Dictionary<Type, IType> CommonTypes { get; } = new Dictionary<Type, IType>
        {
            [typeof(ObjectType<Attachment>)] = new ObjectType<Attachment>(),
            [typeof(InputObjectType<FileValueInput>)] = new InputObjectType<FileValueInput>(),
        };
        
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

        public T GetType<T>() where T : IType
        {
            return (T) CommonTypes[typeof(T)];
        }

        public T GetType<T>(Type type) where T : IType
        {
            return (T) CommonTypes[type];
        }
    }
}