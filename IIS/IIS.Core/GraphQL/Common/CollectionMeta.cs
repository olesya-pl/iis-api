namespace IIS.Core.GraphQL.Common
{
    public class CollectionMeta
    {
        public int Total { get; }
        public CollectionMeta(int total)
        {
            Total = total;
        }
    }
}