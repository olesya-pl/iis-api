using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    [Obsolete("From legacy version of database with same relations to different types")]
    public class OutputUnionType : UnionType
    {
        private readonly IEnumerable<ObjectType> _objectTypes;

        private readonly EntityType _source;
        private readonly string _relationName;

        public OutputUnionType(EntityType source, string relationName, IEnumerable<ObjectType> objectTypes)
        {
            _source = source;
            _relationName = relationName;
            _objectTypes = objectTypes;
            if (_objectTypes.Count() < 2)
                throw new ArgumentException("Two or more types are required to create a union.");
        }

        public static string GetName(EntityType source, string relationName)
        {
            return $"{OntologyObjectType.GetName(source)}_{relationName}_Union";
        }

        protected override void Configure(IUnionTypeDescriptor d)
        {
            d.Name(GetName(_source, _relationName));
            foreach (var type in _objectTypes)
                d.Type(type);
        }
    }

    // ----- Mutator types ----- //

    // ----- Mutator CUD types ----- //
}
