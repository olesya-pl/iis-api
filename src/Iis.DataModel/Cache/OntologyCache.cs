using Iis.DataModel.Materials;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Cache
{
    public class OntologyCache : IOntologyCache
    {
        OntologyContext _context;
        Dictionary<Guid, MaterialSignEntity> _materialSignsDict;
        Dictionary<string, IReadOnlyCollection<string>> _fieldNamesByType = new Dictionary<string, IReadOnlyCollection<string>>();
        public IReadOnlyCollection<MaterialSignEntity> MaterialSigns => _materialSignsDict.Values;
        public OntologyCache(OntologyContext context)
        {
            if (context == null) return;
            _context = context;
            _materialSignsDict = _context.MaterialSigns
                .Include(ms => ms.MaterialSignType)
                .ToDictionaryAsync(ms => ms.Id, ms => ms)
                .Result;
        }

        public MaterialSignEntity GetMaterialSign(Guid id)
        {
            return _materialSignsDict[id];
        }        
    }
}
