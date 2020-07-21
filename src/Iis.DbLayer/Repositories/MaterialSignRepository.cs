using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Iis.DataModel.Cache;
using Iis.DataModel.Materials;

namespace Iis.DbLayer.Repositories
{
    public class MaterialSignRepository : IMaterialSignRepository
    {
        private readonly IOntologyCache _ontologyCache;
        public MaterialSignRepository(IOntologyCache ontologyCache)
        {
            _ontologyCache = ontologyCache;
        }
        public MaterialSignEntity GetById(Guid id)
        {
            return _ontologyCache.GetMaterialSign(id);
        }

        public MaterialSignEntity GetByValue(string value)
        {
            if(string.IsNullOrWhiteSpace(value)) return null;

            return _ontologyCache.MaterialSigns
                            .FirstOrDefault(e => e.Title == value);
        }

        public IReadOnlyCollection<MaterialSignEntity> GetAllByTypeName(string typeName)
        {
            return _ontologyCache.MaterialSigns
                                .Where(e => e.MaterialSignType.Name == typeName)
                                .OrderBy(e => e.OrderNumber)
                                .ToList();
        }
    }
}