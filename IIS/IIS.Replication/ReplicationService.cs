using System;
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

        public void IndexEntity(string message)
        {
            var json = JObject.Parse(message);
            var index = json["type"].Value<string>().ToLower();
            var entity = json["entity"].ToString();
            var id = json["entity"]["id"].Value<string>();
            var postData = PostData.String(entity);
            var indexResponse =  _elasticClient.LowLevel.IndexPut<StringResponse>(index, id, postData);
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
                        foreach (var target in arrRelation.Instances)
                        {
                            var attr = (Attribute)target;
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
                        foreach (var target in arrayRelation.Instances)
                        {
                            var iobj = ToJObject((Entity)target);
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
    }
}
