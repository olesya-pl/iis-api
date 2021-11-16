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
using Iis.DbLayer.MaterialDictionaries;
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
using Microsoft.Extensions.Logging;
using Iis.Interfaces.Materials;

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
        private readonly ILogger<ThemeService<IIISUnitOfWork>> _logger;

        public ThemeService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            OntologyContext context,
            IMapper mapper,
            IOntologySchema ontologySchema,
            IElasticService elasticService,
            IMaterialElasticService materialElasticService,
            IReportElasticService reportService,
            IElasticState elasticState,
            IImageVectorizer imageVectorizer,
            IMaterialProvider materialProvider,
            ILogger<ThemeService<IIISUnitOfWork>> logger) : base(unitOfWorkFactory)
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
            _logger = logger;
        }

        public async Task<Guid> CreateThemeAsync(ThemeDto theme)
        {
            var entity = _mapper.Map<ThemeEntity>(theme);
            
            entity.QueryResults 
                = entity.ReadQueryResults 
                = (await GetQueryResultsAsync(entity.Id, entity.UserId, entity.TypeId, ThemeQueryParser.Parse(entity.QueryRequest))).Count;
            entity.UnreadCount = default;
                
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
                entity.QueryResults 
                    = entity.ReadQueryResults 
                    = (await GetQueryResultsAsync(entity.Id, entity.UserId, entity.TypeId, ThemeQueryParser.Parse(entity.QueryRequest))).Count;
            }
            
            entity.UnreadCount = GetUnreadQueryResult(originalEntity.QueryResults, originalEntity.ReadQueryResults);
        }

        private int GetUnreadQueryResult(int queryResults, int readQueryResults)
        {
            return queryResults - readQueryResults > 0
                ? queryResults - readQueryResults
                : 0;
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

            entity.UnreadCount = GetUnreadQueryResult(entity.QueryResults, entity.ReadQueryResults);

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

        public async Task<IEnumerable<ThemeDto>> GetThemesByUserIdAsync(Guid userId, PaginationParams paginationParams, SortingParams sorting)
        {
            var query  = GetThemes().Where(e => e.UserId == userId);
            var (skip, take) = paginationParams.ToEFPage();

            var entities = await ApplySorting(query, sorting.ColumnName, sorting.Order)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

            var themes = _mapper.Map<IEnumerable<ThemeDto>>(entities);

            return themes;
        }
        public async Task<IReadOnlyCollection<ThemeDto>> GetAllThemesByUserIdAsync(Guid userId)
        {
            var query  = GetThemes().Where(e => e.UserId == userId);

            var entities = await query.ToListAsync();

            var themes = _mapper.Map<IReadOnlyCollection<ThemeDto>>(entities);

            return themes;
        }

        public async Task<IReadOnlyCollection<ThemeDto>> GetAllThemesByEntityTypeNamesAsync(Guid userId, IReadOnlyCollection<string> entityTypeNames)
        {
            var entityTypes = await RunWithoutCommitAsync(
                async uow => await uow.ThemeRepository.GetThemeTypesByEntityTypeNamesAsync(entityTypeNames));
            
            var entitiesTypeId = entityTypes.Select(e => e.Id);
            
            var entities  = await GetThemes()
                .Where(e => e.UserId == userId && entitiesTypeId.Contains(e.TypeId))
                .OrderByDescending(_ => _.UpdatedAt)
                .ToListAsync();
            
            var themes = _mapper.Map<IReadOnlyCollection<ThemeDto>>(entities);

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
            var themes = await _context.Themes
                .Where(p => themeTypes.Contains(p.TypeId))
                .AsNoTracking()
                .ToListAsync();

            var themesByQuery = themes
                .ToDictionary(k => k.Id, x => new {
                    x.Id,
                    Query = ThemeQueryParser.Parse(x.QueryRequest),
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
                    _context.Entry(theme.OriginalEntity).State = EntityState.Modified;
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
                ("comment", SortDirections.ASC) => query
                    .OrderBy(e => e.Comment)
                    .ThenBy(e => e.Id),
                ("comment", SortDirections.DESC) => query
                    .OrderByDescending(e => e.Comment)
                    .ThenBy(e => e.Id),
                ("type", SortDirections.ASC) => query
                    .OrderBy(e => e.Type)
                    .ThenBy(e => e.Id),
                ("type", SortDirections.DESC) => query
                    .OrderByDescending(e => e.Type)
                    .ThenBy(e => e.Id),
                ("queryResults", SortDirections.ASC) => query
                    .OrderBy(e => e.QueryResults)
                    .ThenBy(e => e.Id),
                ("queryResults", SortDirections.DESC) => query
                    .OrderByDescending(e => e.QueryResults)
                    .ThenBy(e => e.Id),
                ("unreadCount", SortDirections.ASC) => query
                    .OrderBy(e => e.UnreadCount)
                    .ThenBy(e => e.Id),
                ("unreadCount", SortDirections.DESC) => query
                    .OrderByDescending(e => e.UnreadCount)
                    .ThenBy(e => e.Id),
                _ => query
                    .OrderByDescending(e => e.UpdatedAt)
                    .ThenBy(e => e.Id)
            };
        }

        private async Task<QueryResult> GetQueryResultsAsync(Guid themeId, Guid userId, Guid typeId, ThemeQuery query)
        {
            _logger.LogInformation("ThemeService. Calculating query results for theme {themeId}", themeId);
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

            try
            {
                if (typeId == ThemeTypeEntity.EntityEventId)
                {
                    var count = await _elasticService.CountEntitiesByConfiguredFieldsAsync(indexes, filter);
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
                            null,
                            new PaginationParams(1, 50),
                            null);
                        return new QueryResult
                        {
                            Count = materialsResults.Count,
                            ThemeId = themeId,
                            TypeId = typeId
                        };
                    }

                    var page = new PaginationParams(1, 50);
                    var count = await _materialElasticService.CountMaterialsByConfiguredFieldsAsync(
                        userId,
                        new SearchParams
                        {
                            Page = page,
                            Suggestion = filter.Suggestion,
                            CherryPickedItems = filter.CherryPickedItems,
                            FilteredItems = filter.FilteredItems
                        });
                    return new QueryResult
                    {
                        Count = count,
                        ThemeId = themeId,
                        TypeId = typeId
                    };
                }
                else if (typeId == ThemeTypeEntity.EntityReportId)
                {
                    var count = await _reportService.CountAsync(new ReportSearchParams
                    {
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
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError("ThemeService. Exception occured while updating theme count {e}", e);
                return new QueryResult
                {
                    Count = 0,
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
    }
}