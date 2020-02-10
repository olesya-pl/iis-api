using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class ShortRelation
    {
        public Guid NodeTypeId { get; set; }
        public Guid? RelationTypeId { get; set; }
    }
}
