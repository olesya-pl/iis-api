namespace IIS.Core.GraphQL.Materials
{
    public abstract class BaseResult
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
    }
}