namespace IIS.Core.GraphQL.Common
{
    public class CollectionMeta
    {
        public CollectionMeta(int total)
        {
            Total = total;
        }

        public int Total { get; }
    }
}
