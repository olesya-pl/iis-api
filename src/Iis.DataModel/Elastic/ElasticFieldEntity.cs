using Iis.Interfaces.Elastic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Elastic
{
    public class ElasticFieldEntity : BaseEntity, IElasticFieldEntity, IIisElasticField
    {
        public string TypeName { get; set; }
        public ElasticObjectType ObjectType { get; set; }
        public string Name { get; set; }
        public bool IsExcluded { get; set; }
        public int Fuzziness { get; set; }
        public decimal Boost { get; set; }
    }
}
