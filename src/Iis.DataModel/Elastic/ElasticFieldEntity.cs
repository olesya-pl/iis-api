using Iis.Interfaces.Elastic;

namespace Iis.DataModel.Elastic
{
    public class ElasticFieldEntity : BaseEntity, IElasticFieldEntity
    {
        public string TypeName { get; set; }
        public ElasticObjectType ObjectType { get; set; }
        public string Name { get; set; }
        public bool IsExcluded { get; set; }
        public int Fuzziness { get; set; }
        public decimal Boost { get; set; }
    }
}
