using System.Collections.Generic;

namespace Iis.DataModel.Materials
{
    public class MaterialSourceAliasEntity : BaseEntity
    {
        public string Source { get; set; }
        public string Alias { get; set; }

        public List<MaterialEntity> Materials { get; set; }
    }
}