using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Themes;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.ThemeManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Interfaces;
using System.Threading;
using IIS.Repository;
using Iis.DbLayer.Repositories;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Params;

namespace Iis.ThemeManagement
{
    public class ThemeService<TUnitOfWork> : BaseService<TUnitOfWork> where TUnitOfWork : IIISUnitOfWork
    {
        private const string CoordinatesPrefix = "__coordinates:*";
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IOntologyModel _ontology;
        private readonly IElasticService _elasticService;
        private readonly IMaterialElasticService _materialElasticService;
        private readonly IElasticState _elasticState;

        public ThemeService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            OntologyContext context, 
            IMapper mapper, 
            IOntologyModel ontology, 
            IElasticService elasticService, 
            IMaterialElasticService materialElasticService,
            IElasticState elasticState) : base(unitOfWorkFactory)
        {
            _context = context;
            _mapper = mapper;
            _ontology = ontology;
            _elasticService = elasticService;
            _materialElasticService = materialElasticService;
            _elasticState = elasticState;
        }

        public async Task<Guid> CreateThemeAsync(Theme theme)
        {
            var entity = _mapper.Map<ThemeEntity>(theme);
            entity.QueryResults = entity.ReadQueryResults = (await GetQueryResultsAsync(entity.TypeId, entity.Query)).Count;

            _context.Themes.Add(entity);

            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<Guid> UpdateThemeAsync(Theme theme)
        {
            PopulateOptionalFields(theme);
            var entity = _mapper.Map<ThemeEntity>(theme);
            _context.Themes.Update(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        private void PopulateOptionalFields(Theme theme)
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
                    theme.Type = new ThemeType { Id = existingEntity.TypeId };
                }
            }
        }

        public async Task<Theme> SetReadCount(Guid themeId, int readCount)
        {
            var entity = await RunWithoutCommitAsync(async uow => await uow.ThemeRepository.GetByIdAsync(themeId));

            if (entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

            entity.ReadQueryResults = readCount;
            var theme = _mapper.Map<Theme>(entity);

            await RunAsync(uow => uow.ThemeRepository.Update(entity));

            return theme;
        }

        public async Task<Theme> DeleteThemeAsync(Guid themeId)
        {
            var entity = await GetThemes()
                            .SingleOrDefaultAsync(e => e.Id == themeId);

            if (entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

            var theme = _mapper.Map<Theme>(entity);

            entity.Type = null;
            entity.User = null;

            _context.Themes.Remove(entity);

            await _context.SaveChangesAsync();

            return theme;
        }

        public async Task<Theme> GetThemeAsync(Guid themeId)
        {
            var entity = await GetThemes()
                            .SingleOrDefaultAsync(e => e.Id == themeId);

            if (entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

            return _mapper.Map<Theme>(entity);
        }

        public async Task<IEnumerable<Theme>> GetThemesByUserIdAsync(Guid userId)
        {
            var entities = await GetThemes()
                                    .Where(e => e.UserId == userId)
                                    .ToListAsync();

            var themes = _mapper.Map<IEnumerable<Theme>>(entities);
            return themes;
        }

        public async Task<ThemeType> GetThemeTypeByEntityTypeNameAsync(string entityTypeName)
        {
            var entity = await _context.ThemeTypes
                                    .SingleOrDefaultAsync(e => e.EntityTypeName == entityTypeName);

            if (entity is null) throw new ArgumentException($"ThemeType does not exist for EntityTypeName = {entityTypeName}");

            return _mapper.Map<ThemeType>(entity);
        }

        public async Task<IEnumerable<ThemeType>> GetThemeTypesAsync()
        {
            var entities = await _context.ThemeTypes
                                            .ToListAsync();

            return _mapper.Map<IEnumerable<ThemeType>>(entities);
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
                .ToList()
                .GroupBy(x => new { x.Query, x.TypeId });

            var tasks = new List<Task<QueryResult>>();

            foreach (var groupedTheme in themesByQuery)
            {
                ct.ThrowIfCancellationRequested();
                tasks.Add(GetQueryResultsAsync(groupedTheme.Key.TypeId, groupedTheme.Key.Query));                
            }

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                var groupedTheme = themesByQuery.FirstOrDefault(p => 
                    string.Equals(p.Key.Query, result.Query, StringComparison.Ordinal) 
                    && p.Key.TypeId == result.TypeId);

                if (groupedTheme == null)
                {
                    continue;
                }

                var newCount = result.Count;

                foreach (var theme in groupedTheme)
                {
                    if (theme.QueryResults != newCount)
                    {
                        theme.QueryResults = newCount;
                        _context.Entry(theme).State = EntityState.Modified;
                    }
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

        private async Task<QueryResult> GetQueryResultsAsync(Guid typeId, string query)
        {
            var filter = new ElasticFilter
            {
                Limit = 50,
                Offset = 0,
                Suggestion = query
            };

            var indexes = typeId switch
            {
                _ when typeId == ThemeTypeEntity.EntityMaterialId => _elasticState.MaterialIndexes,
                _ when typeId == ThemeTypeEntity.EntityObjectId || typeId == ThemeTypeEntity.EntityMapId => GetOntologyIndexes(_ontology, "ObjectOfStudy"),
                _ when typeId == ThemeTypeEntity.EntityEventId => GetOntologyIndexes(_ontology, "Event"),
                _ => (IEnumerable<string>)null
            };

            if (indexes is null) return new QueryResult { 
                Count = 0,
                Query = query,
                TypeId = typeId
            };

            if (typeId == ThemeTypeEntity.EntityEventId)
            {
                var count = await _elasticService.CountByAllFieldsAsync(indexes, filter);
                return new QueryResult
                {
                    Count = count,
                    Query = query,
                    TypeId = typeId
                };
            }
            else if (typeId == ThemeTypeEntity.EntityMaterialId)
            {
                var count = await _materialElasticService.CountMaterialsByConfiguredFieldsAsync(new SearchParams{Limit = filter.Limit, Offset = filter.Offset, Suggestion = filter.Suggestion});
                return new QueryResult
                {
                    Count = count,
                    Query = query,
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
                    Query = query,
                    TypeId = typeId
                };
            }
        }

        private string[] GetOntologyIndexes(IOntologyModel ontology, string typeName)
        {
            var types = ontology.EntityTypes.Where(p => p.Name == typeName);

            return types
                .SelectMany(e => _ontology.GetChildTypes(e))
                .Concat(types)
                .Where(e => e.IsAbstract)
                .Select(e => e.Name)
                .Distinct()
                .ToArray();
        }

        private struct QueryResult
        {
            public int Count { get; set; }
            public string Query { get; set; }
            public Guid TypeId { get; set; }
        }
    }
}