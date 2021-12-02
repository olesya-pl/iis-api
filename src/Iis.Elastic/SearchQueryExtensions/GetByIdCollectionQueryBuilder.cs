using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class GetByIdCollectionQueryBuilder : BaseQueryBuilder<GetByIdCollectionQueryBuilder>
    {
        private const string ValuesPropertyName = "values";
        private const string IdsProperyName = "ids";
        private IReadOnlyCollection<Guid> _idCollection;
        public GetByIdCollectionQueryBuilder(IReadOnlyCollection<Guid> idCollection)
        {
            if (idCollection is null || !idCollection.Any())
            {
                throw new ArgumentException("Parameter should not be null or empty.", nameof(idCollection));
            }

            _idCollection = idCollection;
        }
        protected override JObject CreateQuery(JObject json)
        {
            json.Property(FromPropertyName, StringComparison.Ordinal).Remove();
            json.Property(SizePropertyName, StringComparison.Ordinal).Remove();

            var idCollection = _idCollection
                                .Select(_ => _.ToString("N"))
                                .ToArray();

            var valuesObject = new JObject(
                                new JProperty(ValuesPropertyName, new JArray(idCollection)));

            var idsObject = new JObject(
                                new JProperty(IdsProperyName, valuesObject));

            json[QueryPropertyName] = idsObject;

            return json;
        }
    }
}