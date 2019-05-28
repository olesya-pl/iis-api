using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using IIS.Core;
using Nest;
using Newtonsoft.Json.Linq;
using Attribute = IIS.Core.Attribute;

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

        public async Task IndexEntityAsync(Entity entity)
        {
            var obj = ToJObject(entity);
            var json = obj.ToString();
            var index = entity.Schema.Name.ToLower();
            var id = entity.Id.ToString();
            var postData = PostData.String(json);
            var indexResponse = await _elasticClient.LowLevel.IndexPutAsync<StringResponse>(index, id, postData);
        }

        private static JObject ToJObject(Entity entity)
        {
            var result = new JObject();
            var type = entity.Schema;
            foreach (var name in type.ConstraintNames)
            {
                var constraint = entity.Schema.GetConstraint(name);
                var prop = new JProperty(name);
                if (type.HasAttribute(name))
                {
                    if (constraint.IsArray)
                    {
                        var arrRelation = entity.GetArrayRelation(name);
                        var array = new JArray();
                        foreach (var relation in arrRelation.Relations)
                        {
                            var attr = (Attribute)relation.Target;
                            var obj = new JObject();
                            obj.Add("id", JToken.FromObject(attr.Id));
                            obj.Add("value", JToken.FromObject(attr.Value));
                            array.Add(obj);
                        }
                    }
                    else
                    {
                        var relation = entity.GetSingleRelation(name);
                        var attr = (Attribute)relation.Target;
                        prop.Value = JToken.FromObject(attr.Value);
                    }
                }
                else if (type.HasEntity(name))
                {
                    if (constraint.IsArray)
                    {
                        var arrayRelation = entity.GetArrayRelation(name);
                        var array = new JArray();
                        foreach (var relation in arrayRelation.Relations)
                        {
                            var iobj = ToJObject((Entity)relation.Target);
                            array.Add(iobj);
                        }
                        prop.Value = array;
                    }
                    else
                    {
                        var relation = entity.GetSingleRelation(name);
                        if (relation != null)
                        {
                            var target = (Entity)relation.Target;
                            var iobj = ToJObject(target);
                            prop.Value = iobj;
                        }
                        else prop.Value = null;
                    }
                }
                else if (type.HasUnion(name))
                {
                    continue;
                }
                result.Add(prop);
            }
            return result;
        }

        public async Task CreateIndexAsync(TypeEntity schema)
        {
            foreach (var constraintName in schema.ConstraintNames)
            {
                if (constraintName == "_relationInfo") continue;
                var indexName = constraintName.ToLower();
                var constraint = schema.GetConstraint(constraintName);
                if (_elasticClient.IndexExists(indexName).Exists)
                    await _elasticClient.DeleteIndexAsync(indexName); // todo: use update
                await _elasticClient.CreateIndexAsync(indexName, desc => GetIndexRequest(desc, constraint));
            }
        }

        private ICreateIndexRequest GetIndexRequest(CreateIndexDescriptor descriptor, Constraint constraint)
        {
            var type = (TypeEntity)constraint.Target;
            descriptor.Map(d => GetTypeMapping(d, type));
            return descriptor;
        }

        private ITypeMapping GetTypeMapping(TypeMappingDescriptor<object> arg, TypeEntity type)
        {
            arg.Properties(d => GetPropertiesSelector(d, type, 3));
            return arg;
        }

        private IPromise<IProperties> GetPropertiesSelector(PropertiesDescriptor<object> arg, TypeEntity type, int depth)
        {
            if (depth == 0) return arg;

            foreach (var constraintName in type.ConstraintNames)
            {
                if (type.HasAttribute(constraintName))
                {
                    var isIndexed = true;//prop.Value.Value<bool>("_indexed");
                    var attribute = type.GetAttribute(constraintName);
                    if (attribute.Type == ScalarType.String) arg.Text(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Int) arg.Number(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Date) arg.Date(s => s.Name(constraintName).Index(isIndexed));
                }
                else if (type.HasEntity(constraintName))
                {
                    var target = type.GetEntity(constraintName);
                    //var value = map.GetValue(constraintName) as JObject;
                    arg.Object<object>(d =>
                        d.Name(constraintName)
                        .Properties(p => GetPropertiesSelector(p, target, depth - 1))
                    );
                }
                else if (type.HasUnion(constraintName))
                {
                    var union = type.GetUnion(constraintName);
                    //var value = map.GetValue(constraintName) as JObject;
                    foreach (var target in union.Classes)
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
