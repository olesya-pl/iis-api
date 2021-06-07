using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Themes;
using Iis.Interfaces.Elastic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.Services.Contracts.Interfaces;
using System.Threading;
using IIS.Repository;
using Iis.DbLayer.Repositories;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Params;
using Iis.Services.Contracts.Dtos;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Ontology.Schema;
using Iis.Domain.Users;
using Newtonsoft.Json;
using IIS.Services.Contracts.Interfaces;

namespace Iis.Services
{
    public class ThemeService<TUnitOfWork> : BaseService<TUnitOfWork>, IThemeService where TUnitOfWork : IIISUnitOfWork
    {
        private const string CoordinatesPrefix = "__coordinates:*";
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IOntologySchema _ontologySchema;
        private readonly IElasticService _elasticService;
        private readonly IMaterialElasticService _materialElasticService;
        private readonly IReportElasticService _reportService;
        private readonly IElasticState _elasticState;
        private readonly IImageVectorizer _imageVectorizer;
        private readonly IMaterialProvider _materialProvider;

        public ThemeService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            OntologyContext context,
            IMapper mapper,
            IOntologySchema ontologySchema,
            IElasticService elasticService,
            IMaterialElasticService materialElasticService,
            IReportElasticService reportService,
            IElasticState elasticState,
            IImageVectorizer imageVectorizer,
            IMaterialProvider materialProvider) : base(unitOfWorkFactory)
        {
            _context = context;
            _mapper = mapper;
            _ontologySchema = ontologySchema;
            _elasticService = elasticService;
            _materialElasticService = materialElasticService;
            _reportService = reportService;
            _elasticState = elasticState;
            _imageVectorizer = imageVectorizer;
            _materialProvider = materialProvider;                 
        }

        public async Task<Guid> CreateThemeAsync(ThemeDto theme)
        {
            var entity = _mapper.Map<ThemeEntity>(theme);
            entity.QueryResults = entity.ReadQueryResults = (await GetQueryResultsAsync(entity.Id, entity.UserId, entity.TypeId, GetQuery(entity.QueryRequest))).Count;

            _context.Themes.Add(entity);

            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<Guid> UpdateThemeAsync(ThemeDto theme)
        {
            PopulateOptionalFields(theme);
            var entity = _mapper.Map<ThemeEntity>(theme);
            await UpdateQueryResults(entity);
            _context.Themes.Update(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        private async Task UpdateQueryResults(ThemeEntity entity)
        {
            var originalEntity = await GetThemeAsync(entity.Id);
            if (string.Equals(originalEntity.QueryRequest, entity.QueryRequest, StringComparison.Ordinal))
            {
                entity.QueryResults = originalEntity.QueryResults;
                entity.ReadQueryResults = originalEntity.ReadQueryResults;
            }
            else
            {
                entity.QueryResults = entity.ReadQueryResults = (await GetQueryResultsAsync(entity.Id, entity.UserId, entity.TypeId, GetQuery(entity.QueryRequest))).Count;
            }
        }

        private void PopulateOptionalFields(ThemeDto theme)
        {
            if (theme.User == null || theme.Type == null)
            {
                var existingEntity = _context.Themes
                    .AsNoTracking()
                    .FirstOrDefault(p => p.Id == theme.Id);
                if (existingEntity == null)
                {
                    throw new ArgumentNullException("Unable to find given theme");
                }

                if (theme.User == null)
                {
                    theme.User = new User { Id = existingEntity.UserId };
                }

                if (theme.Type == null)
                {
                    theme.Type = new ThemeTypeDto { Id = existingEntity.TypeId };
                }
            }
        }

        public async Task<ThemeDto> SetReadCount(Guid themeId, int readCount)
        {
            var entity = await RunWithoutCommitAsync(async uow => await uow.ThemeRepository.GetByIdAsync(themeId));

            if (entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

            entity.ReadQueryResults = readCount;
            var theme = _mapper.Map<ThemeDto>(entity);

            await RunAsync(uow => uow.ThemeRepository.Update(entity));

            return theme;
        }

        public async Task<ThemeDto> DeleteThemeAsync(Guid themeId)
        {
            var entity = await GetThemes()
                            .SingleOrDefaultAsync(e => e.Id == themeId);

            if (entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

            var theme = _mapper.Map<ThemeDto>(entity);

            entity.Type = null;
            entity.User = null;

            _context.Themes.Remove(entity);

            await _context.SaveChangesAsync();

            return theme;
        }

        public async Task<ThemeDto> GetThemeAsync(Guid themeId)
        {
            var entity = await GetThemes()
                            .SingleOrDefaultAsync(e => e.Id == themeId);

            if (entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

            return _mapper.Map<ThemeDto>(entity);
        }

        public async Task<IEnumerable<ThemeDto>> GetThemesByUserIdAsync(Guid userId, SortingParams sorting)
        {
            var query  = GetThemes().Where(e => e.UserId == userId);

            var entities = await ApplySorting(query, sorting.ColumnName, sorting.Order)
                                    .ToListAsync();

            var themes = _mapper.Map<IEnumerable<ThemeDto>>(entities);

            return themes;
        }

        public async Task<ThemeTypeDto> GetThemeTypeByEntityTypeNameAsync(string entityTypeName)
        {
            var entity = await _context.ThemeTypes
                                    .SingleOrDefaultAsync(e => e.EntityTypeName == entityTypeName);

            if (entity is null) throw new ArgumentException($"ThemeType does not exist for EntityTypeName = {entityTypeName}");

            return _mapper.Map<ThemeTypeDto>(entity);
        }

        public async Task<IEnumerable<ThemeTypeDto>> GetThemeTypesAsync()
        {
            var entities = await _context.ThemeTypes
                                            .ToListAsync();

            return _mapper.Map<IEnumerable<ThemeTypeDto>>(entities);
        }
        public async Task UpdateQueryResultsAsync(CancellationToken ct, params Guid[] themeTypes)
        {
            var themesQuery = _context.Themes
                .AsNoTracking();

            if (themeTypes != null && themeTypes.Any())
            {
                themesQuery = themesQuery.Where(p => themeTypes.Contains(p.TypeId));
            }

            var themesByQuery = themesQuery
                .ToDictionary(k => k.Id, x => new {
                    x.Id,
                    Query = GetQuery(x.QueryRequest),
                    x.TypeId,
                    x.UserId,
                    OriginalEntity = x
                });

            var tasks = new List<Task<QueryResult>>();

            foreach (var groupedTheme in themesByQuery.Values)
            {
                ct.ThrowIfCancellationRequested();
                tasks.Add(GetQueryResultsAsync(groupedTheme.Id, groupedTheme.UserId, groupedTheme.TypeId, groupedTheme.Query));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                if (!themesByQuery.ContainsKey(result.ThemeId))
                {
                    continue;
                }

                var theme = themesByQuery[result.ThemeId];
                var newCount = result.Count;
                if (theme.OriginalEntity.QueryResults != newCount)
                {
                    theme.OriginalEntity.QueryResults = newCount;
                    _context.Entry(theme).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync(ct);
        }

        private IQueryable<ThemeEntity> GetThemes()
        {
            return _context.Themes
                    .Include(e => e.Type)
                    .Include(e => e.User)
                    .AsNoTracking();
        }

        private static IQueryable<ThemeEntity> ApplySorting(IQueryable<ThemeEntity> query, string columnName, string order)
        {
            return (columnName, order) switch
            {
                ("updatedAt", "asc") => query.OrderBy(e => e.UpdatedAt),
                ("updatedAt", "desc") => query.OrderByDescending(e => e.UpdatedAt),
                _ => query.OrderBy(e => e.Id)
            };
        }

        private async Task<QueryResult> GetQueryResultsAsync(Guid themeId, Guid userId, Guid typeId, Query query)
        {
            if (query == null)
            {
                return new QueryResult
                {
                    Count = 0,
                    ThemeId = themeId,
                    TypeId = typeId
                };
            }
            
            var filter = new ElasticFilter
            {
                Limit = 50,
                Offset = 0,
                Suggestion = query.Suggestion,
                CherryPickedItems = query
                    .CherryPickedItems
                    .ToList(),
                FilteredItems = query.FilteredItems.ToList()
            };

            var indexes = typeId switch
            {
                _ when typeId == ThemeTypeEntity.EntityMaterialId => _elasticState.MaterialIndexes,
                _ when typeId == ThemeTypeEntity.EntityObjectId || typeId == ThemeTypeEntity.EntityMapId => GetOntologyIndexes(EntityTypeNames.ObjectOfStudy.ToString()),
                _ when typeId == ThemeTypeEntity.EntityEventId => GetOntologyIndexes(EntityTypeNames.Event.ToString()),
                _ when typeId == ThemeTypeEntity.EntityReportId => new[] { _elasticState.ReportIndex },
                _ => (IEnumerable<string>)null
            };

            if (indexes is null) return new QueryResult
            {
                Count = 0,
                ThemeId = themeId,
                TypeId = typeId
            };

            if (typeId == ThemeTypeEntity.EntityEventId)
            {
                var count = await _elasticService.CountByAllFieldsAsync(indexes, filter);
                return new QueryResult
                {
                    Count = count,
                    ThemeId = themeId,
                    TypeId = typeId
                };
            }
            else if (typeId == ThemeTypeEntity.EntityMaterialId)
            {
                if (query.SearchByImageInput != null && query.SearchByImageInput.HasConditions)
                {
                    var content = Convert.FromBase64String(query.SearchByImageInput.Content);
                    var imageVectorList = await _imageVectorizer.VectorizeImage(content, query.SearchByImageInput.Name);
                    if (imageVectorList.Any())
                    {
                        var searchResult = await _materialElasticService.SearchByImageVector(userId, imageVectorList, new PaginationParams(1, 50));
                        return new QueryResult
                        {
                            Count = searchResult.Count,
                            ThemeId = themeId,
                            TypeId = typeId
                        };
                    }
                    else
                    {
                        return new QueryResult
                        {
                            Count = 0,
                            ThemeId = themeId,
                            TypeId = typeId
                        };
                    }
                }
                if (query.SearchByRelation != null && query.SearchByRelation.HasConditions) 
                {
                    var materialsResults = await _materialProvider.GetMaterialsCommonForEntitiesAsync(
                        userId,
                        query.SearchByRelation.NodeIdentityList,
                        query.SearchByRelation.IncludeDescendants,
                        query.Suggestion,
                        new PaginationParams(1,50),
                        null);
                    return new QueryResult
                    {
                        Count = materialsResults.Count,
                        ThemeId = themeId,
                        TypeId = typeId
                    };
                }
                
                var page = new PaginationParams(1, 50);
                var count = await _materialElasticService.CountMaterialsByConfiguredFieldsAsync(userId, new SearchParams { Page = page, Suggestion = filter.Suggestion });
                return new QueryResult
                {
                    Count = count,
                    ThemeId = themeId,
                    TypeId = typeId
                };
            }
            else if (typeId == ThemeTypeEntity.EntityReportId)
            {
                var count = await _reportService.CountAsync(new ReportSearchParams { 
                    Suggestion = filter.Suggestion
                });
                return new QueryResult
                {
                    Count = count,
                    ThemeId = themeId,
                    TypeId = typeId
                };
            }
            else
            {
                if (typeId == ThemeTypeEntity.EntityMapId)
                {
                    filter.Suggestion = filter.Suggestion.Contains(CoordinatesPrefix)
                        ? filter.Suggestion
                        : $"{CoordinatesPrefix} && {filter.Suggestion}";
                }

                var count = await _elasticService.CountEntitiesByConfiguredFieldsAsync(indexes, filter);
                return new QueryResult
                {
                    Count = count,
                    ThemeId = themeId,
                    TypeId = typeId
                };
            }
        }

        private string[] GetOntologyIndexes(string typeName)
        {
            var type = _ontologySchema.GetEntityTypeByName(typeName);

            return type
                .GetAllDescendants()
                .Concat(new[] { type })
                .Where(e => !e.IsAbstract)
                .Select(e => e.Name)
                .Distinct()
                .ToArray();
        }

        private struct QueryResult
        {
            public int Count { get; set; }
            public Guid ThemeId { get; set; }
            public Guid TypeId { get; set; }
        }

        private Query GetQuery(string queryRequest) 
        {
            try
            {
                var queryResult = JObject.Parse(queryRequest);
                if (queryRequest == null)
                {
                    return null;
                }
                var suggestion = string.Empty;
                if (queryResult.ContainsKey("suggestion"))
                {
                    suggestion = queryResult["suggestion"].Value<string>();                    
                }

                var cherryPickedItems = Enumerable.Empty<CherryPickedItem>().ToList();
                if (queryResult.ContainsKey("selectedEntities"))
                {
                    cherryPickedItems = queryResult.SelectToken("selectedEntities", false)
                        .AsEnumerable()
                        .Select(p => new CherryPickedItem(p.Value<string>("id"), p.Value<bool>("includeDescendants")))
                        .ToList();
                }

                SearchByImageInput searchByImageInput = null;
                if (queryResult.ContainsKey("searchByImageInput"))
                {
                    var searchByImageInputJson = queryResult["searchByImageInput"] as JObject;

                    if (searchByImageInputJson != null)
                    {
                        searchByImageInput = searchByImageInputJson.ToObject<SearchByImageInput>();
                    }
                }

                SearchByRelationInput searchByRelation = null;
                if (queryResult.ContainsKey("searchByRelation"))
                {
                    var searchByRelationInputJson = queryResult["searchByRelation"] as JObject;

                    if (searchByRelationInputJson != null)
                    {
                        searchByRelation = searchByRelationInputJson.ToObject<SearchByRelationInput>();
                    }
                }

                var filteredItems = Array.Empty<Property>();

                if (queryResult.ContainsKey("filteredItems"))
                {
                    var filteredItemsJson = queryResult["filteredItems"] as JArray;
                    if (filteredItemsJson != null)
                    {
                        filteredItems = filteredItemsJson.ToObject<Property[]>();
                    }
                }              
                
                return new Query
                {
                    Suggestion = suggestion,
                    CherryPickedItems = cherryPickedItems,
                    SearchByImageInput = searchByImageInput,
                    SearchByRelation = searchByRelation,
                    FilteredItems = filteredItems
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private class Query
        {
            public string Suggestion { get; set; }
            public IReadOnlyCollection<CherryPickedItem> CherryPickedItems { get; set; } = new List<CherryPickedItem>();
            public IReadOnlyCollection<Property> FilteredItems { get; set; } = new List<Property>();
            public SearchByImageInput SearchByImageInput { get; set; }
            public SearchByRelationInput SearchByRelation { get; set; }
        }

        private class SearchByImageInput
        {
            public string Name { get; set; }
            public string Content { get; set; }
            [JsonIgnore]
            public bool HasConditions => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Content);
        }

        private class SearchByRelationInput
        {
            public IEnumerable<Guid> NodeIdentityList { get; set; }
            public bool IncludeDescendants { get; set; }
            [JsonIgnore]
            public bool HasConditions => NodeIdentityList != null && NodeIdentityList.Any();
        }
    }
}