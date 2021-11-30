using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Common;
using Iis.DbLayer.Extensions;
using Iis.DbLayer.MaterialEnum;
using Iis.Domain.Materials;
using IIS.Repository;
using Iis.Services.Contracts.Materials.Distribution;
using Microsoft.EntityFrameworkCore;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;

namespace Iis.DbLayer.Repositories
{
    public class MaterialRepository : RepositoryBase<OntologyContext>, IMaterialRepository
    {
        private static readonly MaterialIncludeEnum[] IncludeAll = new MaterialIncludeEnum[]
        {
            MaterialIncludeEnum.WithChildren,
            MaterialIncludeEnum.WithFeatures
        };

        private IQueryable<MaterialEntity> Materials => Context.Materials
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
            .Include(m => m.Editor);

        public Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQueryAsNoTracking(includes)
                    .SingleOrDefaultAsync(e => e.Id == id);
        }

        public Task<MaterialEntity[]> GetByIdsAsync(ISet<Guid> ids, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQueryAsNoTracking(includes)
                    .Where(e => ids.Contains(e.Id))
                    .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQueryAsNoTracking(includes)
                            .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQueryAsNoTracking(includes)
                .Take(limit)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, int offset, params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQueryAsNoTracking(includes)
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
            var result = await GetMaterialsQueryAsNoTracking(IncludeAll)
                .OnlyParent()
                .Where(_ => materialIdList.Contains(_.Id))
                .ToArrayAsync();

            return result;
        }

        public Task<PaginatedCollection<MaterialEntity>> GetAllAsync(
            int limit,
            int offset,
            string sortColumnName = null,
            string sortOrder = null) =>
            GetAllWithPredicateAsync(limit, offset, sortColumnName: sortColumnName, sortOrder: sortOrder);

        public Task<PaginatedCollection<MaterialEntity>> GetAllAsync(
            IEnumerable<Guid> materialIdList,
            int limit,
            int offset,
            string sortColumnName = null,
            string sortOrder = null) =>
            GetAllWithPredicateAsync(limit, offset, e => materialIdList.Contains(e.Id), sortColumnName, sortOrder);

        public Task<PaginatedCollection<MaterialEntity>> GetAllAsync(
            IEnumerable<string> types,
            int limit,
            int offset,
            string sortColumnName = null,
            string sortOrder = null) =>
            GetAllWithPredicateAsync(limit, offset, e => types.Contains(e.Type), sortColumnName, sortOrder);

        public async Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId)
        {
            return await GetMaterialsQueryAsNoTracking(MaterialIncludeEnum.OnlyParent)
                .Where(p => p.MaterialAssignees.Any(_ => _.AssigneeId == assigneeId))
                .ToArrayAsync();
        }

        public Task<MaterialAccessEntity> GetMaterialAccessByIdAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            return Context.Materials
                .AsNoTracking()
                .Include(_ => _.ProcessedStatus)
                .Include(_ => _.Editor)
                .Select(_ => new MaterialAccessEntity
                {
                    Id = _.Id,
                    EditorId = _.EditorId,
                    Editor = _.Editor,
                    ProcessedStatusSignId = _.ProcessedStatusSignId,
                    ProcessedStatus = _.ProcessedStatus,
                    AccessLevel = _.AccessLevel
                })
                .SingleOrDefaultAsync(e => e.Id == materialId, cancellationToken);
        }

        public void AddMaterialEntity(MaterialEntity materialEntity)
        {
            Context.Materials.Add(materialEntity);
        }

        public void AddMaterialInfos(IEnumerable<MaterialInfoEntity> materialEntities)
        {
            Context.MaterialInfos.AddRange(materialEntities);
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

        public async Task EditMaterialAsync(Guid id, Action<MaterialEntity> editAction, params MaterialIncludeEnum[] includes)
        {
            var materialEntity = await GetMaterialsQuery(includes)
                .SingleOrDefaultAsync(_ => _.Id == id);
            if (materialEntity == null)
                throw new ArgumentNullException(nameof(id), "Material with given id not found");

            editAction(materialEntity);

            materialEntity.UpdatedAt = DateTime.UtcNow;
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
            return (await GetMaterialsQueryAsNoTracking(includes)
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material)
                .ToArrayAsync())
                .Distinct(new EqualityByIdComparer())
                .Select(baseEntity => (MaterialEntity)baseEntity)
                .ToList();
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
            return await GetMaterialsQueryAsNoTracking(MaterialIncludeEnum.WithChildren)
                    .Where(e => e.ParentId == materialId)
                    .Select(e => e.Id)
                    .ToArrayAsync();
        }

        public Task<bool> CheckMaterialExistsAndHasContent(Guid materialId)
        {
            return Materials
                .AsNoTracking()
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
            return GetMaterialsQueryAsNoTracking(IncludeAll).CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<MaterialDistributionItem>> GetMaterialsForDistribution(
            UserDistributionItem user,
            Expression<Func<MaterialEntity, bool>> filter)
        {
            var query = GetMaterialsForDistributionQuery().Where(_ => _.AccessLevel <= user.AccessLevel);

            if (filter != null) query = query.Where(filter);

            if (user.Channels.Count > 0) query = query.Where(_ => _.Channel == null || user.Channels.Contains(_.Channel));

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

        public void RemoveMaterialAndRelatedData(Guid materialId)
        {
            var materials = Context.Materials.Where(p => p.Id == materialId || p.ParentId == materialId);
            if (materials.Any())
            {
                Context.Materials.RemoveRange(materials);
            }
            var changeHistory = Context.ChangeHistory.Where(p => p.TargetId == materialId);
            if (changeHistory.Any()) {
                Context.ChangeHistory.RemoveRange(changeHistory);
            }
        }

        public async Task<IReadOnlyList<ResCallerReceiverDto>> GetCallInfo(IReadOnlyList<Guid> nodeIds)
        {
            var rawData = await
                    (from mf in Context.MaterialFeatures
                    join mi in Context.MaterialInfos on mf.MaterialInfoId equals mi.Id
                    where (mf.NodeLinkType == MaterialNodeLinkType.Caller ||
                            mf.NodeLinkType == MaterialNodeLinkType.Receiver)
                        && nodeIds.Contains(mf.NodeId)
                    orderby mi.MaterialId, mf.NodeLinkType
                    select new ResRawLinkDto
                    {
                        MaterialId = mi.MaterialId,
                        NodeId = mf.NodeId,
                        NodeLinkType = mf.NodeLinkType
                    }).ToArrayAsync();

            var groupedData = rawData
                .GroupBy(rd => rd.MaterialId)
                .Where(g => g.Count() == 2)
                .Select(g => (CallerId: g.First().NodeId, ReceiverId: g.Last().NodeId))
                .ToArray();

            var result = groupedData
                .GroupBy(_ => _)
                .Select(g => new ResCallerReceiverDto
                {
                    CallerId = g.Key.CallerId,
                    ReceiverId = g.Key.ReceiverId,
                    Count = g.Count()
                })
                .ToList();

            return result;
        }

        private Task<PaginatedCollection<MaterialEntity>> GetAllWithPredicateAsync(
            int limit,
            int offset,
            Expression<Func<MaterialEntity, bool>> predicate = null,
            string sortColumnName = null,
            string sortOrder = null)
        {
            return GetMaterialsQueryAsNoTracking(IncludeAll)
                .OnlyParent()
                .WhereOrDefault(predicate)
                .ApplySorting(sortColumnName, sortOrder)
                .AsPaginatedCollectionAsync(offset, limit);
        }

        private IQueryable<MaterialEntity> GetOnlyMaterialsForNodeIdListQuery(IEnumerable<Guid> nodeIdList)
        {
            return Context.Materials
                        .Join(
                            Context.MaterialInfos,
                            m => m.Id,
                            mi => mi.MaterialId,
                            (material, materialInfo) => new { Material = material, MaterialInfo = materialInfo })
                        .Join(
                            Context.MaterialFeatures,
                            m => m.MaterialInfo.Id,
                            mf => mf.MaterialInfoId,
                            (materialInfoJoined, materialFeature) => new { MaterialInfoJoined = materialInfoJoined, MaterialFeature = materialFeature })
                        .Where(m => nodeIdList.Contains(m.MaterialFeature.NodeId))
                        .Select(m => m.MaterialInfoJoined.Material);
        }

        private IQueryable<MaterialEntity> GetMaterialsQueryAsNoTracking(MaterialIncludeEnum include) =>
           Materials.AsNoTracking().Include(include);

        private IQueryable<MaterialEntity> GetMaterialsQueryAsNoTracking(MaterialIncludeEnum[] includes) =>
            Materials.AsNoTracking().Include(includes);

        private IQueryable<MaterialEntity> GetMaterialsQuery(MaterialIncludeEnum[] includes) =>
            Materials.Include(includes);

        private IQueryable<MaterialEntity> GetMaterialsForDistributionQuery() =>
           Context.Materials
               .Where(_ => (_.ProcessedStatusSignId == null
                       || _.ProcessedStatusSignId == MaterialEntity.ProcessingStatusNotProcessedSignId)
                   && !_.MaterialAssignees.Any()
                   && _.ParentId == null);
    }
}