namespace Iis.Interfaces.Elastic
{
    public class ElasticBulkResponse 
    {
        public string Id { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorReason { get; set; }

        public string ErrorType { get; set; }

        public string SuccessOperation { get; set; }
    }
}
