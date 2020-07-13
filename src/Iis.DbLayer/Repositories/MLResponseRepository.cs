using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.DataModel.Materials;

namespace Iis.DbLayer.Repositories
{
    public class MLResponseRepository : IMLResponseRepository
    {
        private readonly OntologyContext _context;

        public MLResponseRepository(OntologyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MLResponseEntity>> GetAllForMaterialAsync(Guid materialId)
        {
            return await _context.MLResponses
                            .Where(p => p.MaterialId == materialId)
                            .AsNoTracking()
                            .ToArrayAsync();
        }
        
        public async Task<MLResponseEntity> SaveAsync(MLResponseEntity entity)
        {
            _context.Add(entity);

            await _context.SaveChangesAsync();

            return entity;
        }
    }
}