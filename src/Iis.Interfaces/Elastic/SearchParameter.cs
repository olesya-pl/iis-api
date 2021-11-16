using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public class SearchParameter
    {
        public SearchParameter(
            string query,
            IReadOnlyList<IIisElasticField> fields,
            bool isExactQuery = false)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (fields is null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            Query = query;
            Fields = fields;
            IsExact = isExactQuery;
        }

        public string Query { get; }
        public IReadOnlyList<IIisElasticField> Fields { get; }
        public bool IsExact { get; }
    }
}