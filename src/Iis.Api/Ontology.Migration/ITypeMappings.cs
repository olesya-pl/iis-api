using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public interface ITypeMappings
    {
        bool IsMapped(Guid nodeTypeId);
    }
}
