using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Legacy.EntityFramework;
using Newtonsoft.Json.Linq;
using Type = IIS.Core.Ontology.EntityFramework.Context.Type;

namespace IIS.Core.GraphQL
{
    public class DummyMutation
    {
        private static JObject _jobject;

        [GraphQLType(typeof(JsonScalarType))]
        public JObject SaveObject([GraphQLType(typeof(JsonScalarType))] JObject data)
        {
            var old = _jobject;
            _jobject = data;
            return old;
        }

        [GraphQLType(typeof(GeoJsonScalarType))]
        public JObject SaveGeo([GraphQLType(typeof(GeoJsonScalarType))]
            JObject data)
        {
            var old = _jobject;
            _jobject = data;
            return old;
        }

        public string RaiseException()
        {
            throw new Exception("Whoops.");
        }

        public string ProcessJsonScalar([GraphQLType(typeof(JsonScalarType))] JObject input)
        {
            return $"ok: {input}";
        }

        public string ProcessComplexJsonScalar(ComplexInput input)
        {
            return $"ok: {input.Inner}";
        }

        public async Task<string> LongTaskAsync(CancellationToken cancellationToken)
        {
            // seems like cancellation token is not supported this way
            await Task.Delay(10000, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return "Task cancelled";
            return "Task completed";
            // dummy commit
        }
    }

    public class ComplexInput
    {
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Inner { get; set; }
    }
}
