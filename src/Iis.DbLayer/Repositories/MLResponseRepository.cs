using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.DataModel.Materials;

namespace Iis.DbLayer.Repositories
{
    internal class MLResponseRepository : IMLResponseRepository
    {
        private readonly OntologyContext _context;

        public MLResponseRepository(OntologyContext context)
        {
            _context = context;
        }

        public async Task<List<MLResponseEntity>> GetAllForMaterialAsync(Guid materialId)
        {
            return await _context.MLResponses
                            .Where(e => e.MaterialId == materialId)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MLResponseEntity>> GetAllForMaterialListAsync(IReadOnlyCollection<Guid> materialIdList)
        {
            return await _context.MLResponses
                            .Where(e => materialIdList.Contains(e.MaterialId))
                            .AsNoTracking()
                            .ToArrayAsync();
        }

        public async Task<IEnumerable<(Guid MaterialId, int Count)>> GetAllForMaterialsAsync(IReadOnlyCollection<Guid> materialIdList)
        {
            if(materialIdList is null || !materialIdList.Any()) return new List<(Guid MaterialId, int Count)>();

            var result = await _context.MLResponses
                                .Where(e => materialIdList.Contains(e.MaterialId))
                                .GroupBy(e => e.MaterialId)
                                .Select(ge => new { MaterialId = ge.Key, Count = ge.Count()})
                                .AsNoTracking()
                                .ToArrayAsync();

            return result
                    .Select(e => (MaterialId:e.MaterialId, Count: e.Count))
                    .ToArray();
        }


        public async Task<MLResponseEntity> SaveAsync(MLResponseEntity entity)
        {
            _context.Add(entity);

            await _context.SaveChangesAsync();

            return entity;
        }
    }
}