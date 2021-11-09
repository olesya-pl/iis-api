using System;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Microsoft.EntityFrameworkCore;

namespace Iis.DataModel.Cache
{
    public class OntologyCache : IOntologyCache
    {
        private readonly OntologyContext _context;
        private readonly Dictionary<Guid, MaterialSignEntity> _materialSignsDict;

        public OntologyCache(OntologyContext context)
        {
            if (context == null) return;
            _context = context;
            _materialSignsDict = _context.MaterialSigns
                .Include(ms => ms.MaterialSignType)
                .ToDictionaryAsync(ms => ms.Id, ms => ms)
                .Result;
        }

        public IReadOnlyCollection<MaterialSignEntity> MaterialSigns => _materialSignsDict.Values;

        public MaterialSignEntity GetMaterialSign(Guid id)
        {
            return _materialSignsDict[id];
        }
    }
}
