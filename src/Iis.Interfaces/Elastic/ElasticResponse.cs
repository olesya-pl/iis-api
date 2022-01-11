namespace Iis.Interfaces.Elastic
{
    public class ElasticResponse
    {
        public bool IsSuccess { get; set; }

        public string ErrorReason { get; set; }

        public string ErrorType { get; set; }
    }
}
