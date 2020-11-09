namespace Iis.Interfaces.Elastic
{
    public class ElasticBulkResponse: ElasticResponse
    {
        public string Id { get; set; }

        public string SuccessOperation { get; set; }
    }
}
