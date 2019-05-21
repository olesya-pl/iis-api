using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core;
using Nest;
using Newtonsoft.Json.Linq;

namespace IIS.Replication
{
    public class ReplicationService : IReplicationService
    {
        private readonly IElasticClient _elasticClient;

        public ReplicationService(string connectionString)
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
            arg.Properties(d => GetPropertiesSelector(d, type, type.IndexConfig));
            return arg;
        }

        private IPromise<IProperties> GetPropertiesSelector(PropertiesDescriptor<object> arg, TypeEntity type, JObject map)
        {
            var props = map.Children<JProperty>();
            if (!props.Any()) return arg;

            foreach (var prop in props)
            {
                var constraintName = prop.Name;
                if (type.HasAttribute(constraintName))
                {
                    var isIndexed = prop.Value.Value<bool>("_indexed");
                    var constraint = type.GetAttribute(constraintName);
                    if (constraint.Type == ScalarType.String) arg.Text(s => s.Name(constraintName).Index(isIndexed));
                    else if (constraint.Type == ScalarType.Int) arg.Number(s => s.Name(constraintName).Index(isIndexed));
                    else if (constraint.Type == ScalarType.Date) arg.Date(s => s.Name(constraintName).Index(isIndexed));
                }
                else if (type.HasEntity(constraintName))
                {
                    var constraint = type.GetEntity(constraintName);
                    var value = map.GetValue(constraintName) as JObject;
                    arg.Object<object>(d =>
                        d.Name(constraintName)
                        .Properties(p => GetPropertiesSelector(p, constraint.Target, value as JObject))
                    );
                }
                else if (type.HasUnion(constraintName))
                {
                    var constraint = type.GetUnion(constraintName);
                    var value = map.GetValue(constraintName) as JObject;
                    foreach (var target in constraint.Targets)
                    {
                        arg.Object<object>(d =>
                            d.Name($"{constraintName}_{target.Name}")
                            .Properties(p => GetPropertiesSelector(p, target, value))
                        );
                    }
                }
            }
            return arg;
        }
    }
}
