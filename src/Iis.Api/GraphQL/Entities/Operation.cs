namespace IIS.Core.GraphQL.Entities
{
    public enum Operation : byte
    {
        Read,
        Create,
        Update,
        Delete
    }

    public static class OperationExtensions
    {
        public static string Short(this Operation operation)
        {
            return operation.ToString().Substring(0, 1);
        }

        public static string ToLower(this Operation operation)
        {
            return operation.ToString().ToLower();
        }
    }
}
