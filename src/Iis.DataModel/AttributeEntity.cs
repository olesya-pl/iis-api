using System;

namespace Iis.DataModel
{
    public class AttributeEntity : BaseEntity
    {
        public virtual NodeEntity Node { get; set; }

        public string Value { get; set; }
    }
}
