using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using OScalarType = IIS.Core.Ontology.ScalarType;
using HCScalarType = HotChocolate.Types.ScalarType;

namespace IIS.Core.GraphQL.Entities
{
    public abstract class TypeStorage
    {
        public Dictionary<Type, Dictionary<string, INamedType>> InputTypes;

        public Dictionary<Type, INamedType> CommonTypes { get; }

        public Dictionary<OScalarType, HCScalarType> Scalars { get; }

        public IEnumerable<INamedType> AllTypes => InputTypes.Values.SelectMany(d => d.Values)
            .Union(Scalars.Values).Union(CommonTypes.Values);

        public TypeStorage()
        {
            InputTypes = new Dictionary<Type, Dictionary<string, INamedType>>();
            CommonTypes = new Dictionary<Type, INamedType>();
            Scalars = new Dictionary<OScalarType, HCScalarType>
            {
                [OScalarType.String] = new StringType(),
                [OScalarType.Integer] = new IntType(),
                [OScalarType.Boolean] = new BooleanType(),
                [OScalarType.Decimal] = new DecimalType(),
                [OScalarType.DateTime] = new DateTimeType(), // HotChocolate uses ISO-8601 by default
                [OScalarType.Geo] = new AnyType(),
                //[OScalarType.Geo] = new GeoJsonScalarType(),
                //            [OScalarType.File] = null, // Implemented as a dirty hack in Extensions.AttributeType(), because File attribute is not scalar
            };
        }

        public T GetOrCreate<T>(string name, Func<T> creator)
            where T : INamedType
        {
            var type = typeof(T);
            if (!InputTypes.TryGetValue(type, out var dict))
            {
                dict = new Dictionary<string, INamedType>();
                InputTypes.Add(type, dict);
            }
            else if (dict.ContainsKey(name))
            {
                return (T)dict[name];
            }

            dict.Add(name, null); // Place empty record to avoid recursive calls on object creation
            var result = creator();
            dict[name] = result; // replace empty record with actual value
            return result;
        }

        public T GetType<T>(string name)
            where T : INamedType
        {
            return (T)InputTypes[typeof(T)][name];
        }

        public T GetType<T>()
            where T : INamedType, new()
        {
            if (!CommonTypes.ContainsKey(typeof(T)))
                CommonTypes.Add(typeof(T), new T());
            return (T)CommonTypes[typeof(T)];
        }
    }
}
