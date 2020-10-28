using System.Linq;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialDictionaries;

namespace Iis.DbLayer.Extensions
{
    internal static class MaterialQueryExtensions
    {
        public static IQueryable<MaterialEntity> WithChildren(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery.Include(m => m.Children);
        }

        public static IQueryable<MaterialEntity> WithFeatures(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                .Include(m => m.MaterialInfos)
                .ThenInclude(m => m.MaterialFeatures);
        }

        public static IQueryable<MaterialEntity> WithNodes(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                .Include(m => m.MaterialInfos)
                .ThenInclude(m => m.MaterialFeatures)
                .ThenInclude(m => m.Node);
        }

        public static IQueryable<MaterialEntity> OnlyParent(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                    .Where(p => p.ParentId == null);
        }
        public static IQueryable<MaterialEntity> ApplySorting(
            this IQueryable<MaterialEntity> materialsQuery,
            string sortColumnName,
            string sortOrder)
        {
            var orderedQueryable = (sortColumnName, sortOrder)
            switch
            {
                (MaterialSortingFields.Type, SortDirections.ASC) => materialsQuery.OrderBy(p => p.Type),
                (MaterialSortingFields.Type, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.Type),
                (MaterialSortingFields.Source, SortDirections.ASC) => materialsQuery.OrderBy(p => p.Source),
                (MaterialSortingFields.Source, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.Source),
                (MaterialSortingFields.Title, SortDirections.ASC) => materialsQuery.OrderBy(p => p.Title),
                (MaterialSortingFields.Title, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.Title),
                (MaterialSortingFields.Importance, SortDirections.ASC) => materialsQuery.OrderBy(p => p.ImportanceSignId),
                (MaterialSortingFields.Importance, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.ImportanceSignId),
                (MaterialSortingFields.ProcessedStatus, SortDirections.ASC) => materialsQuery.OrderBy(p => p.ProcessedStatusSignId),
                (MaterialSortingFields.ProcessedStatus, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.ProcessedStatusSignId),
                (MaterialSortingFields.Nodes, SortDirections.ASC) => materialsQuery.OrderBy(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),
                (MaterialSortingFields.Nodes, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),
                (MaterialSortingFields.CreatedDate, SortDirections.ASC) => materialsQuery.OrderBy(p => p.CreatedDate),
                (MaterialSortingFields.SessionPriority, SortDirections.ASC) => materialsQuery.OrderBy(p => p.SessionPriorityId),
                (MaterialSortingFields.SessionPriority, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.SessionPriorityId),
                _ => materialsQuery.OrderByDescending(p => p.CreatedDate),
            };
            return orderedQueryable.ThenBy(p => p.Id);
        }

    }
}