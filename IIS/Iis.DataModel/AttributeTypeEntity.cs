namespace Iis.DataModel
{
    public class AttributeTypeEntity : BaseEntity
    {
        public virtual NodeTypeEntity NodeType { get; set; }

        public ScalarType ScalarType { get; set; }
    }
}
