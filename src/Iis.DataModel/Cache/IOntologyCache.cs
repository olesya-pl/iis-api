using Iis.DataModel.Materials;
using System;
using System.Collections.Generic;

namespace Iis.DataModel.Cache
{
    public interface IOntologyCache
    {
        IReadOnlyCollection<MaterialSignEntity> MaterialSigns { get; }
        MaterialSignEntity GetMaterialSign(Guid id);
    }
}