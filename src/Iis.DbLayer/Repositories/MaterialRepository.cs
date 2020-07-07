using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Extensions;
using Iis.DbLayer.MaterialEnum;

namespace Iis.DbLayer.Repository
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly OntologyContext _context;
        public MaterialRepository(OntologyContext context)
        {
            _context = context;
        }

        public Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQuery(includes)
                    .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                            .ToArrayAsync();
        }
        
        public async Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId)
        {
            return await GetMaterialsQuery(MaterialIncludeEnum.OnlyParent)
                            .Where(p => p.AssigneeId == assigneeId)
                            .ToArrayAsync();
        }
        
        public async Task<IEnumerable<MLResponseEntity>> GetMachineLearningResultsForMaterialAsync(Guid materialId)
        {
            return await _context.MLResponses
                            .Where(p => p.MaterialId == materialId)
                            .AsNoTracking()
                            .ToArrayAsync();
        }
        
        private IQueryable<MaterialEntity> GetSimplifiedMaterialsQuery()
        {
            return _context.Materials
                    .Include(m => m.Importance)
                    .Include(m => m.Reliability)
                    .Include(m => m.Relevance)
                    .Include(m => m.Completeness)
                    .Include(m => m.SourceReliability)
                    .Include(m => m.ProcessedStatus)
                    .Include(m => m.SessionPriority)
                    .Include(m => m.Assignee)
                    .AsNoTracking();
        }
        
        private IQueryable<MaterialEntity> GetMaterialsQuery(params MaterialIncludeEnum[] includes)
        {
            if(!includes.Any()) return GetSimplifiedMaterialsQuery();
            
            includes = includes.Distinct()
                                .ToArray();

            var resultQuery = GetSimplifiedMaterialsQuery();

            foreach (var include in includes)
            {
                resultQuery = include switch
                {
                    MaterialIncludeEnum.WithFeatures => resultQuery.WithFeatures(),
                    MaterialIncludeEnum.WithChildren => resultQuery.WithChildren(),
                    _ => resultQuery
                };
            }

            return resultQuery;
        }
   
    }
}