using System.Collections.Generic;

namespace Iis.MaterialLoader.Models
{
    public class UpdateFieldData
    {
        public string Name { get; set; }

        public List<FieldValue> Values { get; set; }
    }
}