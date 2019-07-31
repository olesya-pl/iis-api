namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public enum Operation
    {
        Read,
        Create,
        Update,
        Delete,
    }
    
    public static class OperationExtensions
    {
        public static string Short(this Operation operation) => operation.ToString().Substring(0, 1);
        public static string ToLower(this Operation operation) => operation.ToString().ToLower();
    }
}