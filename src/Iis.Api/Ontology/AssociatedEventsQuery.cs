using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotChocolate;
using Iis.Domain;
using IIS.Core.GraphQL.Common;
using Iis.Api.GraphQL.Common;
using Iis.Interfaces.Elastic;

namespace Iis.Api.Ontology
{
    public class AssociatedEventsQuery
    {
        private const string EventImportanceColumn = "eventImportance";
        private const string EventStateColumn = "eventState";
        private const string StartAtColumn = "startAt";
        private const string NameColumn = "name";
        private const string UpdatedAtColumn = "updatedAt";
        public async Task<GraphQLCollection<EventAssociatedWithEntity>> GetEventsAssociatedWithEntity(
            [Service] IOntologyService ontologyService,
            [Service] NodeMapper nodeToJObjectMapper,
            [GraphQLNonNullType] Guid entityId,
            [GraphQLNonNullType] PaginationInput pagination,
            SortingInput sorting)
        {
            var sortingParam = new SortingParams(sorting?.ColumnName ?? UpdatedAtColumn, sorting?.Order ?? SortingParams.DESC);

            var paginationParam = new PaginationParams(pagination.Page, pagination.PageSize);

            var entities = ontologyService.GetEventsAssociatedWithEntity(entityId);

            var rawResult = entities
                            .Select(_ => nodeToJObjectMapper.ToEventToAssociatedWithEntity(_));

            var sortedResult = GetSortedAndPaginatedCollection(rawResult, sortingParam, paginationParam);

            return await Task.FromResult(new GraphQLCollection<EventAssociatedWithEntity>(sortedResult, rawResult.Count()));
        }

        private static EventAssociatedWithEntity[] GetSortedAndPaginatedCollection(IEnumerable<EventAssociatedWithEntity> source, SortingParams sorting, PaginationParams pagination)
        {
            var page = pagination.ToEFPage();
            return ApplySorting(source, sorting)
                    .Skip(page.Skip)
                    .Take(page.Take)
                    .ToArray();
        }

        private static IEnumerable<EventAssociatedWithEntity> ApplySorting(IEnumerable<EventAssociatedWithEntity> sourceQuery, SortingParams sorting)
        {
            return (sorting.ColumnName, sorting.Order) switch
            {
                (EventImportanceColumn, SortingParams.ASC) => sourceQuery.OrderBy(_ => _.Importance.Code),
                (EventImportanceColumn, SortingParams.DESC) => sourceQuery.OrderByDescending(_ => _.Importance.Code),
                (EventStateColumn, SortingParams.ASC) => sourceQuery.OrderBy(_ => _.State.Code),
                (EventStateColumn, SortingParams.DESC) => sourceQuery.OrderByDescending(_ => _.State.Code),
                (StartAtColumn, SortingParams.ASC) => sourceQuery.OrderBy(_ => _.StartsAt),
                (StartAtColumn, SortingParams.DESC) => sourceQuery.OrderByDescending(_ => _.StartsAt),
                (NameColumn, SortingParams.ASC) => sourceQuery.OrderBy(_ => _.Name),
                (NameColumn, SortingParams.DESC) => sourceQuery.OrderByDescending(_ => _.Name),
                (UpdatedAtColumn, SortingParams.ASC) => sourceQuery.OrderBy(_ => _.UpdatedAt),
                (UpdatedAtColumn, SortingParams.DESC) => sourceQuery.OrderByDescending(_ => _.UpdatedAt),
                _ => sourceQuery.OrderByDescending(_ => _.UpdatedAt)
            };
        }
    }
}