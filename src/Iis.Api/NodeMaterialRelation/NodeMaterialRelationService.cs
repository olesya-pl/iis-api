using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.Elastic;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Repository;
using IIS.Repository.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace IIS.Core.NodeMaterialRelation
{
    public class NodeMaterialRelationService<TUnitOfWork> : BaseService<TUnitOfWork> where TUnitOfWork : IIISUnitOfWork
    {
        private readonly OntologyContext _context;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly IMaterialElasticService _materialElasticService;

        private const string NodeIdPropertyName = "MaterialFeature.NodeId";

        public NodeMaterialRelationService(OntologyContext context, 
            IMaterialElasticService materialElasticService,
            IChangeHistoryService changeHistoryService,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _context = context;
            _materialElasticService = materialElasticService;
            _changeHistoryService = changeHistoryService;
        }

        public async Task CreateMultipleRelations(Guid userId, string query, Guid nodeId, string userName)
        {
            var materials = await _materialElasticService.BeginSearchByScrollAsync(userId,
                new SearchParams
                {
                    Suggestion = query,
                    Page = new PaginationParams(1, ElasticConstants.MaxItemsCount)
                });

            var materialsCount = materials.Items.Count;
            var materialIds = materials.Items.Keys.ToHashSet();

            await CreateMultipleRelations(new HashSet<Guid>(new[] { nodeId }), materialIds, userName);

            while (materialsCount > 0)
            {
                var scrollId = materials.ScrollId;
                materials = await _materialElasticService.SearchByScroll(userId, scrollId);
                materialsCount = materials.Items.Count;
                materialIds = materials.Items.Keys.ToHashSet();
                await CreateMultipleRelations(new HashSet<Guid>(new[] { nodeId }), materialIds, userName);
            }
        }

        public async Task CreateMultipleRelations(HashSet<Guid> nodeIds, HashSet<Guid> materialIds, string userName)
        {
            var changeHistoryList = new List<ChangeHistoryDto>();
            var tasks = new List<Task>();

            foreach (var nodeId in nodeIds)
            {
                var existingItems = await RunWithoutCommitAsync(uow => uow.NodeMaterialRelationRepository.GetExistingRelationMaterialIds(nodeId, materialIds));
                var newMaterials = materialIds.Except(existingItems).ToList();
                await RunAsync(uow => uow.NodeMaterialRelationRepository.CreateRelations(nodeId, newMaterials));

                foreach (var materialId in materialIds)
                {
                    changeHistoryList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = nodeId.ToString("N"),
                        OldValue = null,
                        PropertyName = NodeIdPropertyName,
                        RequestId = Guid.NewGuid(),
                        TargetId = materialId,
                        UserName = userName
                    });
                }
            }

            await _changeHistoryService.SaveMaterialChanges(changeHistoryList);
        }

        private MaterialEntity GetMaterial(Guid materialId)
        {
            return _context.Materials.SingleOrDefault(e => e.Id == materialId);
        }

        public async Task Delete(NodeMaterialRelation relation, string userName = null)
        {
            var material = GetMaterial(relation.MaterialId);

            var featureToRemove = await _context.MaterialFeatures
                .Include(p => p.MaterialInfo)
                .FirstOrDefaultAsync(p => p.NodeId == relation.NodeId && p.MaterialInfo.MaterialId == relation.MaterialId);
            _context.MaterialInfos.Remove(featureToRemove.MaterialInfo);
            _context.MaterialFeatures.Remove(featureToRemove);
            await _context.SaveChangesAsync();

            var changeHistoryDto = new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = null,
                OldValue = relation.NodeId.ToString("N"),
                PropertyName = NodeIdPropertyName,
                RequestId = Guid.NewGuid(),
                TargetId = relation.MaterialId,
                UserName = userName
            };
            await _changeHistoryService.SaveMaterialChanges(new[] { changeHistoryDto }, material.Title);
        }
    }
}
