namespace IIS.Ontology.EntityFramework
{
    public class EntityKind : ObjectEnum
    {
        public static EntityKind Entity { get; } = new EntityKind("entity");
        public static EntityKind Relation { get; } = new EntityKind("relation");

        protected EntityKind(string value) : base(value) { }

        public static explicit operator EntityKind(string value) => new EntityKind(value);
    }
}
