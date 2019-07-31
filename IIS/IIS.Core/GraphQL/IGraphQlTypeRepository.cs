using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Mutations;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework;
using OScalarType = IIS.Core.Ontology.ScalarType;
using HCScalarType = HotChocolate.Types.ScalarType;
using Type = System.Type;

namespace IIS.Core.GraphQL
{
    public interface IGraphQlTypeRepository
    {
        Dictionary<OScalarType, HCScalarType> Scalars { get; }
        IEnumerable<INamedType> AllTypes { get; }
        T GetType<T>() where T : IType, new();
        T GetType<T>(string name) where T : INamedType;
        T GetOrCreate<T>(string name, Func<T> creator) where T : INamedType;
    }

    public class GraphQlTypeRepository : IGraphQlTypeRepository
    {
        public Dictionary<Type, IType> CommonTypes { get; } = new Dictionary<Type, IType>();
        
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

        public IEnumerable<INamedType> AllTypes => InputTypes.Values.SelectMany(d => d.Values)
            .Union(Scalars.Values);

        public Dictionary<Type, Dictionary<string, INamedType>> InputTypes = new Dictionary<Type, Dictionary<string, INamedType>>();

        public T GetOrCreate<T>(string name, Func<T> creator) where T : INamedType
        {
            var type = typeof(T);
            if (!InputTypes.TryGetValue(type, out var dict))
            {
                dict = new Dictionary<string, INamedType>();
                InputTypes.Add(type, dict);
            }
            else if (dict.ContainsKey(name))
                return (T) dict[name];
            
            var result = creator();
            dict.Add(name, result);
            return result;

        }

        public T GetType<T>(string name) where T : INamedType => (T) InputTypes[typeof(T)][name];

        public T GetType<T>() where T : IType, new()
        {
            if (!CommonTypes.ContainsKey(typeof(T)))
                CommonTypes.Add(typeof(T), new T());
            return (T) CommonTypes[typeof(T)];
        }
    }
}