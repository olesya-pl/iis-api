using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Extensions;
using Iis.DbLayer.MaterialEnum;
using Iis.Domain.Materials;
using IIS.Repository;
using Iis.Services.Contracts.Materials.Distribution;

namespace Iis.DbLayer.Repositories
{
    public class MaterialRepository : RepositoryBase<OntologyContext>, IMaterialRepository
    {
        private static readonly MaterialIncludeEnum[] IncludeAll = new MaterialIncludeEnum[]
        {
            MaterialIncludeEnum.WithChildren,
            MaterialIncludeEnum.WithFeatures
        };

        public Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQuery(includes)
                    .SingleOrDefaultAsync(e => e.Id == id);
        }

        public Task<MaterialEntity[]> GetByIdsAsync(ISet<Guid> ids, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQuery(includes)
                    .Where(e => ids.Contains(e.Id))
                    .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                            .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                .Take(limit)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, int offset, params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                .OrderBy(_ => _.Id)
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllForRelatedNodeListAsync(IEnumerable<Guid> nodeIdList)
        {
            var materialIdList = await GetOnlyMaterialsForNodeIdListQuery(nodeIdList)
                                .Select(e => e.Id)
                                .ToArrayAsync();

            var materialResult = await GetAllAsync(materialIdList, 0, 0);

            return materialResult.Entities;
        }

        public Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null)
        {
            return GetAllWithPredicateAsync(limit, offset, sortColumnName: sortColumnName, sortOrder: sortOrder);
        }

        public Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<Guid> materialIdList, int limit, int offset, string sortColumnName = null, string sortOrder = null)
        {
            return GetAllWithPredicateAsync(limit, offset, e => materialIdList.Contains(e.Id), sortColumnName, sortOrder);
        }

        public Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<string> types, int limit, int offset, string sortColumnName = null, string sortOrder = null)
        {
            return GetAllWithPredicateAsync(limit, offset, e => types.Contains(e.Type), sortColumnName, sortOrder);
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId)
        {
            return await GetMaterialsQuery()
                            .OnlyParent()
                            .Where(p => p.MaterialAssignees.Any(_ => _.AssigneeId == assigneeId))
                            .ToArrayAsync();
        }

        public void AddMaterialEntity(MaterialEntity materialEntity)
        {
            Context.Materials.Add(materialEntity);
        }

        public void AddMaterialInfos(IEnumerable<MaterialInfoEntity> materialEntity)
        {
            Context.MaterialInfos.AddRange(materialEntity);
        }

        public void AddMaterialFeatures(IEnumerable<MaterialFeatureEntity> materialFeatureEntities)
        {
            Context.MaterialFeatures.AddRange(materialFeatureEntities);
        }

        public void AddMaterialAssignees(IEnumerable<MaterialAssigneeEntity> entities)
        {
            Context.MaterialAssignees.AddRange(entities);
        }

        public void RemoveMaterialAssignees(IEnumerable<MaterialAssigneeEntity> entities)
        {
            Context.MaterialAssignees.RemoveRange(entities);
        }

        public void EditMaterial(MaterialEntity materialEntity)
        {
            materialEntity.UpdatedAt = DateTime.UtcNow;
            materialEntity.Editor = null;

            Context.Materials.Update(materialEntity);
        }

        public Task<List<Guid>> GetNodeIsWithMaterialsAsync(IReadOnlyCollection<Guid> nodeIdCollection)
        {
            return Context.Materials
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialFeature.NodeId)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MaterialEntity>> GetMaterialCollectionByNodeIdAsync(IReadOnlyCollection<Guid> nodeIdCollection, params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Guid>> GetMaterialIdCollectionByNodeIdCollectionAsync(IReadOnlyCollection<Guid> nodeIdCollection)
        {
            return await Context.Materials
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material.Id)
                .ToArrayAsync();
        }

        public Task<List<MaterialsCountByType>> GetParentMaterialByNodeIdQueryAsync(IReadOnlyCollection<Guid> nodeIdCollection)
        {
            return Context.Materials
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material).Where(_ => _.ParentId == null)
                .GroupBy(p => p.Type)
                .Select(group => new MaterialsCountByType
                {
                    Count = group.Count(),
                    Type = group.Key
                })
                .ToListAsync();
        }

        public void AddFeatureIdList(Guid materialId, IEnumerable<Guid> featureIdList)
        {
            foreach (var featureId in featureIdList)
                Context.MaterialFeatures.Add(MaterialFeatureEntity.CreateFrom(materialId, featureId));

            Guid firstFeatureId = featureIdList.FirstOrDefault();
            if (firstFeatureId != default)
                Context.MaterialFeatures.Add(MaterialFeatureEntity.CreateFrom(materialId, firstFeatureId, MaterialNodeLinkType.Caller));
        }

        public async Task<IEnumerable<Guid>> GetChildIdListForMaterialAsync(Guid materialId)
        {
            return await GetMaterialsQuery(MaterialIncludeEnum.WithChildren)
                    .Where(e => e.ParentId == materialId)
                    .Select(e => e.Id)
                    .ToArrayAsync();
        }

        public Task<bool> CheckMaterialExistsAndHasContent(Guid materialId)
        {
            return GetMaterialsQuery()
                        .AnyAsync(e => e.Id == materialId && !string.IsNullOrWhiteSpace(e.Content));
        }

        public async Task RemoveMaterialsAndRelatedData(IReadOnlyCollection<Guid> fileIdList)
        {
            var removeFileIdList = fileIdList
                .Select(e => $"'{e.ToString("N")}'")
                .ToArray();

            using (var transaction = await Context.Database.BeginTransactionAsync())
            {
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"LocationHistory\" where \"MaterialId\" is not null");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"MaterialFeatures\"");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"MaterialInfos\"");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"Materials\"");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"MLResponses\"");

                if (removeFileIdList.Any())
                {
                    Context.Database.ExecuteSqlRaw("DELETE FROM public.\"Files\" WHERE \"Id\"::text in ({0})", string.Join(" , ", fileIdList));
                }

                await transaction.CommitAsync();
            }
        }

        public Task<Guid?> GetParentIdByChildIdAsync(Guid materialId)
        {
            return Context.Materials
                .Where(e => e.Id.Equals(materialId))
                .Select(e => e.ParentId).FirstOrDefaultAsync();
        }

        public Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
        {
            return GetMaterialsQuery(IncludeAll).CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<MaterialDistributionItem>> GetMaterialsForDistribution(
            UserDistributionItem user,
            Expression<Func<MaterialEntity, bool>> filter)
        {
            var query = GetMaterialsForDistributionQuery().Where(_ => _.AccessLevel <= user.AccessLevel);

            if (filter != null) query = query.Where(filter);

            if (user.Channels.Count > 0) query = query.Where(_ => user.Channels.Contains(_.Channel));

            var materialEntities = await query
                .OrderByDescending(_ => _.RegistrationDate)
                .Take(user.FreeSlots).ToArrayAsync();

            return materialEntities.Select(_ => new MaterialDistributionItem(_.Id, _.Channel)).ToList();
        }

        public async Task<IReadOnlyList<MaterialChannelMappingEntity>> GetChannelMappingsAsync()
        {
            return await Context.MaterialChannelMappings.ToArrayAsync();
        }

        public async Task SaveDistributionResult(DistributionResult distributionResult)
        {
            var materialIds = distributionResult.Items.Select(_ => _.MaterialId).ToList();
            var materials = await Context.Materials
                .Where(_ => materialIds.Contains(_.Id)).ToArrayAsync();

            foreach (var material in materials)
            {
                var userId = distributionResult.GetUserId(material.Id);
                if (!userId.HasValue)
                    continue;

                var newMaterialAssignee = MaterialAssigneeEntity.CreateFrom(material.Id, userId.Value);
                material.MaterialAssignees.Add(newMaterialAssignee);
            }
        }


        private async Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllWithPredicateAsync(
            int limit = 0,
            int offset = 0,
            Expression<Func<MaterialEntity, bool>> predicate = null,
            string sortColumnName = null,
            string sortOrder = null)
        {
            var materialQuery = predicate is null
                                ? GetMaterialsQuery(IncludeAll)
                                    .OnlyParent()
                                : (IQueryable<MaterialEntity>)
                                    GetMaterialsQuery(IncludeAll)
                                    .OnlyParent()
                                    .Where(predicate);

            var materialCountQuery = materialQuery;

            if (limit == 0)
            {
                materialQuery = materialQuery
                                    .ApplySorting(sortColumnName, sortOrder);
            }
            else
            {
                materialQuery = materialQuery
                                    .ApplySorting(sortColumnName, sortOrder)
                                    .Skip(offset)
                                    .Take(limit);
            }

            var materials = await materialQuery
                                    .ToArrayAsync();

            var materialCount = await materialCountQuery.CountAsync();


            return (materials, materialCount);
        }

        private IQueryable<MaterialEntity> GetSimplifiedMaterialsQuery()
        {
            return Context.Materials
                    .Include(m => m.File)
                    .Include(m => m.Importance)
                    .Include(m => m.Reliability)
                    .Include(m => m.Relevance)
                    .Include(m => m.Completeness)
                    .Include(m => m.SourceReliability)
                    .Include(m => m.ProcessedStatus)
                    .Include(m => m.SessionPriority)
                    .Include(m => m.MaterialAssignees)
                        .ThenInclude(_ => _.Assignee)
                    .Include(m => m.Editor)
                    .AsNoTracking();
        }

        private IQueryable<MaterialEntity> GetOnlyMaterialsForNodeIdListQuery(IEnumerable<Guid> nodeIdList)
        {
            return Context.Materials
                        .Join(Context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                            (Material, MaterialInfo) => new { Material, MaterialInfo })
                        .Join(Context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                            (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                        .Where(m => nodeIdList.Contains(m.MaterialFeature.NodeId))
                        .Select(m => m.MaterialInfoJoined.Material);
        }

        private IQueryable<MaterialEntity> GetMaterialsQuery(params MaterialIncludeEnum[] includes)
        {
            if (!includes.Any()) return GetSimplifiedMaterialsQuery();

            includes = includes.Distinct()
                                .ToArray();

            var resultQuery = GetSimplifiedMaterialsQuery();

            foreach (var include in includes)
            {
                resultQuery = include switch
                {
                    MaterialIncludeEnum.WithFeatures => resultQuery.WithFeatures(),
                    MaterialIncludeEnum.WithNodes => resultQuery.WithNodes(),
                    MaterialIncludeEnum.WithChildren => resultQuery.WithChildren(),
                    MaterialIncludeEnum.WithFiles => resultQuery.WithFiles(),
                    MaterialIncludeEnum.OnlyParent => resultQuery.OnlyParent(),
                    _ => resultQuery
                };
            }

            return resultQuery;
        }

        private IQueryable<MaterialEntity> GetMaterialsForDistributionQuery() =>
           Context.Materials
               .Where(_ => (_.ProcessedStatusSignId == null
                       || _.ProcessedStatusSignId == MaterialEntity.ProcessingStatusNotProcessedSignId)
                   && !_.MaterialAssignees.Any()
                   && _.ParentId == null);
    }
}