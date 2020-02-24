using System;
using System.Collections.Generic;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticCompareResult
    {
        public List<Guid> NeedToDelete { get; set; }
        
        public List<Guid> NeedToUpdate { get; set; }
    }
}