using System;
using System.Collections.Generic;
using Iis.DataModel.Materials;

namespace Iis.DataModel.Cache
{
    public interface IOntologyCache
    {
        IReadOnlyCollection<MaterialSignEntity> MaterialSigns { get; }
        MaterialSignEntity GetMaterialSign(Guid id);
    }
}