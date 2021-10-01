using System.Linq;
using Microsoft.EntityFrameworkCore;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialDictionaries;
using Iis.DbLayer.MaterialEnum;

namespace Iis.DbLayer.Extensions
{
    internal static class MaterialQueryExtensions
    {
        internal class MaterialInfoJoined
        {
            public MaterialEntity Material {get;set;}
            public MaterialInfoEntity MaterialInfo {get;set;}
        }

        internal class MaterialFeatureJoined
        {
            public MaterialInfoJoined MaterialInfoJoined {get;set;}
            public MaterialFeatureEntity MaterialFeature {get;set;}
        }

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

        public static IQueryable<MaterialEntity> WithFiles(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                .Include(m => m.File);
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
                (MaterialSortingFields.Importance, SortDirections.ASC) => materialsQuery.OrderBy(p => p.ImportanceSignId),
                (MaterialSortingFields.Importance, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.ImportanceSignId),
                (MaterialSortingFields.ProcessedStatus, SortDirections.ASC) => materialsQuery.OrderBy(p => p.ProcessedStatusSignId),
                (MaterialSortingFields.ProcessedStatus, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.ProcessedStatusSignId),
                (MaterialSortingFields.Nodes, SortDirections.ASC) => materialsQuery.OrderBy(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),
                (MaterialSortingFields.Nodes, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),
                (MaterialSortingFields.CreatedDate, SortDirections.ASC) => materialsQuery.OrderBy(p => p.CreatedDate),
                (MaterialSortingFields.RegistrationDate, SortDirections.ASC) => materialsQuery.OrderBy(p => p.RegistrationDate),
                (MaterialSortingFields.RegistrationDate, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.RegistrationDate),
                (MaterialSortingFields.SessionPriority, SortDirections.ASC) => materialsQuery.OrderBy(p => p.SessionPriorityId),
                (MaterialSortingFields.SessionPriority, SortDirections.DESC) => materialsQuery.OrderByDescending(p => p.SessionPriorityId),
                _ => materialsQuery.OrderByDescending(p => p.CreatedDate),
            };
            return orderedQueryable.ThenBy(p => p.Id);
        }

        public static IQueryable<MaterialEntity> Include(this IQueryable<MaterialEntity> query, MaterialIncludeEnum include) => include switch
        {
            MaterialIncludeEnum.WithFeatures => query.WithFeatures(),
            MaterialIncludeEnum.WithNodes => query.WithNodes(),
            MaterialIncludeEnum.WithChildren => query.WithChildren(),
            MaterialIncludeEnum.WithFiles => query.WithFiles(),
            MaterialIncludeEnum.OnlyParent => query.OnlyParent(),
            _ => query
        };

        public static IQueryable<MaterialEntity> Include(this IQueryable<MaterialEntity> query, MaterialIncludeEnum[] includes)
        {
            if (includes.Length == 0)
                return query;

            foreach (var include in includes.Distinct())
                query = query.Include(include);

            return query;
        }

        public static IQueryable<MaterialFeatureJoined> JoinMaterialFeaturesAsNoTracking(
            this IQueryable<MaterialEntity> materialsQuery,
            OntologyContext context)
        {
            return materialsQuery
                .Join(context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                    (Material, MaterialInfo) => new MaterialInfoJoined{ Material =  Material, MaterialInfo = MaterialInfo })
                .Join(context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                    (MaterialInfoJoined, MaterialFeature) => new MaterialFeatureJoined {MaterialInfoJoined = MaterialInfoJoined, MaterialFeature = MaterialFeature })
                .AsNoTracking();
        }
    }
}