using Newtonsoft.Json.Linq;

namespace Iis.Elastic
{
    public class ElasticConstants
    {
        public const int MaxItemsCount = 10000;
        public const string SecurityPolicyName = "security-policy";
        public const string DefaultPassword = "123456";
        public const string UsersIndexName = "_xpack/security/user";
        public const int DefaultScrollDurationMinutes = 2;
        public const string CustomSimilarityFunctionName = "discarded_idf";
    }
}
