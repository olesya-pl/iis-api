using HotChocolate;
using Iis.Api.GraphQL.Common;
using Iis.DbLayer.Repositories;
using Iis.Services;
using Iis.Services.Contracts.Dtos;
using IIS.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.NodeMaterialRelation
{
    public class NodeMaterialRelationQuery
    {
        public async Task<List<IdTitle>> GetRelatedNodesForLinkTab(
            [Service] NodeMaterialRelationService<IIISUnitOfWork> relationService,
            Guid materialId)
        {
            var result = await relationService.GetRelatedNodesForLinkTabAsync(materialId);
            return result.Select(r => new IdTitle { Id = r.Id, Title = r.Title }).ToList();
        }
    }
}
