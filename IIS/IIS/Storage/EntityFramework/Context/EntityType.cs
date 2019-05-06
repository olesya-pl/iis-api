namespace IIS.Storage.EntityFramework.Context
{
    public class EntityType : ObjectEnum
    {
        public static EntityType Entity { get; } = new EntityType("entity");
        public static EntityType Relation { get; } = new EntityType("relation");

        protected EntityType(string value) : base(value) { }

        public static explicit operator EntityType(string value) => new EntityType(value);
    }
}
