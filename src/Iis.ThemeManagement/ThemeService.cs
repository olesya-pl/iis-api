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

namespace Iis.ThemeManagement
{
    public class ThemeService
    {
        private const string CoordinatesPrefix = "__coordinates:*";
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IOntologyModel _ontology;
        private readonly IElasticService _elasticService;
        private readonly IElasticState _elasticState;

        public ThemeService(OntologyContext context, IMapper mapper, IOntologyModel ontology, IElasticService elasticService, IElasticState elasticState)
        {
            _context = context;
            _mapper = mapper;
            _ontology = ontology;
            _elasticService = elasticService;
            _elasticState = elasticState;
        }

        public async Task<Guid> CreateThemeAsync(Theme theme)
        {
            var entity = _mapper.Map<ThemeEntity>(theme);

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

        public async Task<Theme> DeleteThemeAsync(Guid themeId)
        {
            var entity = await GetThemes()
                            .SingleOrDefaultAsync(e => e.Id == themeId);

            if(entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

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

            if(entity is null) throw new ArgumentException($"Theme does not exist for id = {themeId}");

            return _mapper.Map<Theme>(entity);
        }

        public async Task<IEnumerable<Theme>> GetThemesByUserIdAsync(Guid userId)
        {
            var entities = await GetThemes()
                                    .Where(e => e.UserId == userId)
                                    .ToListAsync();

            var searchTasks = entities.Select(e => {
                return ExecuteThemeQuery(e.Id, e.Type.Id, e.Query, _ontology);
            });

            var searchResults = await Task.WhenAll(searchTasks);

            var themes = _mapper.Map<IEnumerable<Theme>>(entities);

            foreach (var theme in themes)
            {
                var result = searchResults.FirstOrDefault(r => r.Id == theme.Id);

                theme.QueryResults = result.Count;
            }

            return themes;
        }

        public async Task<ThemeType> GetThemeTypeByEntityTypeNameAsync(string entityTypeName)
        {
            var entity = await _context.ThemeTypes
                                    .SingleOrDefaultAsync(e => e.EntityTypeName == entityTypeName);

            if(entity is null) throw new ArgumentException($"ThemeType does not exist for EntityTypeName = {entityTypeName}");

            return _mapper.Map<ThemeType>(entity);
        }

        public async Task<IEnumerable<ThemeType>> GetThemeTypesAsync()
        {
            var entities = await _context.ThemeTypes
                                            .ToListAsync();

            return _mapper.Map<IEnumerable<ThemeType>>(entities);
        }

        private IQueryable<ThemeEntity> GetThemes()
        {
            return _context.Themes
                    .Include(e => e.Type)
                    .Include(e => e.User)
                    .AsNoTracking();
        }

        private async Task<(Guid Id, int Count)> ExecuteThemeQuery(Guid id, Guid typeId, string query, IOntologyModel ontology)
        {
            var filter = new ElasticFilter
            {
                Limit = 50,
                Offset = 0,
                Suggestion = query
            };

            var indexes = typeId switch
            {
                _ when typeId == ThemeTypeEntity.EntityMaterialId  => _elasticState.MaterialIndexes,
                _ when typeId == ThemeTypeEntity.EntityObjectId || typeId == ThemeTypeEntity.EntityMapId => GetOntologyIndexes(ontology, "ObjectOfStudy"),
                _ when typeId == ThemeTypeEntity.EntityEventId => GetOntologyIndexes(ontology, "Event"),
                _   => (IEnumerable<string>) null
            };

            if(indexes is null) return (Id: id, Count: 0);

            if(typeId == ThemeTypeEntity.EntityEventId)
            {
                var searchResult = await _elasticService.SearchByAllFieldsAsync(indexes,filter);
                return (Id: id, Count: searchResult.count);

            }
            else if (typeId == ThemeTypeEntity.EntityMaterialId)
            {
                var searchResult = await _elasticService.SearchMaterialsByConfiguredFieldsAsync(filter);
                return (Id: id, Count: searchResult.Count);
            }
            else
            {
                if(typeId == ThemeTypeEntity.EntityMapId)
                {
                    filter.Suggestion = filter.Suggestion.Contains(CoordinatesPrefix) ? filter.Suggestion : $"{CoordinatesPrefix} && {filter.Suggestion}"; 
                }

                var searchResult = await _elasticService.SearchEntitiesByConfiguredFieldsAsync(indexes,filter);

                return (Id: id, Count: searchResult.Count);
            }
        }

        private string[] GetOntologyIndexes(IOntologyModel ontology, string typeName)
        {
            var types = ontology.EntityTypes.Where(p => p.Name == typeName);

            return types
                .SelectMany(e => _ontology.GetChildTypes(e))
                .Concat(types)
                .Where(e => e is IEntityTypeModel entityTypeModel && !entityTypeModel.IsAbstract)
                .Select(e => e.Name)
                .Distinct()
                .ToArray();
        }
    }
}