using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.OSchema;
using Nest;

namespace IIS.Search
{
    public interface ISearchService
    {
        Task CreateIndexAsync(TypeEntity schema);
    }

    public class SearchService : ISearchService
    {
        private readonly IElasticClient _elasticClient;

        public SearchService(string connectionString)
        {
            var node = new Uri(connectionString);
            var settings = new ConnectionSettings(node)
                .ThrowExceptions();
            _elasticClient = new ElasticClient(settings);
        }

        public async Task CreateIndexAsync(TypeEntity schema)
        {
            foreach (var constraintName in schema.ConstraintNames)
            {
                if (constraintName == "_relationInfo") continue;
                var indexName = constraintName.ToLower();
                var constraint = schema.GetEntity(constraintName);
                if (_elasticClient.IndexExists(indexName).Exists)
                    await _elasticClient.DeleteIndexAsync(indexName); // todo: use update
                await _elasticClient.CreateIndexAsync(indexName, desc => GetIndexRequest(desc, constraint));
            }
        }

        private ICreateIndexRequest GetIndexRequest(CreateIndexDescriptor descriptor, EntityConstraint constraint)
        {
            var type = constraint.Target;
            descriptor.Map(d => GetTypeMapping(d, type));
            return descriptor;
        }

        private ITypeMapping GetTypeMapping(TypeMappingDescriptor<object> arg, TypeEntity type)
        {
            arg.Properties(d => GetPropertiesSelector(d, type, 4));
            return arg;
        }

        private IPromise<IProperties> GetPropertiesSelector(PropertiesDescriptor<object> arg, TypeEntity type, int depth)
        {
            if (depth == 0) return arg; // temp
            foreach (var constraintName in type.ConstraintNames)
            {
                if (constraintName == "_relationInfo") continue;
                if (type.HasAttribute(constraintName))
                {
                    var constraint = type.GetAttribute(constraintName);
                    if (constraint.Type == ScalarType.String) arg.Text(s => s.Name(constraintName));
                    else if (constraint.Type == ScalarType.Int) arg.Number(s => s.Name(constraintName));
                    else if (constraint.Type == ScalarType.Date) arg.Date(s => s.Name(constraintName));
                }
                else if (type.HasEntity(constraintName))
                {
                    var constraint = type.GetEntity(constraintName);
                    arg.Object<object>(d => 
                        d.Name(constraintName)
                        .Properties(p => GetPropertiesSelector(p, constraint.Target, depth - 1))
                    );
                }
                else if (type.HasUnion(constraintName))
                {
                    var constraint = type.GetUnion(constraintName);
                    foreach (var target in constraint.Targets)
                    {
                        arg.Object<object>(d =>
                            d.Name($"{constraintName}_{target.Name}")
                            .Properties(p => GetPropertiesSelector(p, target, depth - 1))
                        );
                    }
                }
            }

            return arg;
        }
    }
}
