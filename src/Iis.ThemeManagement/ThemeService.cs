using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Newtonsoft.Json.Linq;

using Iis.Domain;
using IIS.Domain;
using Iis.DataModel;
using Iis.DataModel.Themes;
using Iis.Interfaces.Elastic;
using Iis.ThemeManagement.Models;
using Iis.Roles;

namespace Iis.ThemeManagement
{
    public class ThemeService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IOntologyModel _ontology;
        private readonly IElasticService _elasticService;

        public ThemeService(OntologyContext context, IMapper mapper, IOntologyModel ontology, IElasticService elasticService)
        {
            _context = context;
            _mapper = mapper;
            _ontology = ontology;
            _elasticService = elasticService;
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
                return ExecuteThemeQuery(e.Id, e.Type.ShortTitle, e.Query, _ontology);
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

        private async Task<(Guid Id, int Count)> ExecuteThemeQuery(Guid id, string typeKey, string query, IOntologyModel ontology)
        {
            const string Object = "О";
            const string Material = "М";
            const string Event = "П";

            var filter = new ElasticFilter
            {
                Limit = 1000,
                Offset = 0,
                Suggestion = query
            };

            var indexes = typeKey switch
            {
                Material => _elasticService.MaterialIndexes,
                Object => GetOntologyIndexes(ontology, "ObjectOfStudy"),
                Event => GetOntologyIndexes(ontology, "Event"),
                _   => (IEnumerable<string>) null
            };

            if(indexes is null) return (Id: id, Count: 0);

            if(typeKey == Event)
            {
                var searchResult = await _elasticService.SearchByAllFieldsAsync(indexes,filter);
                return (Id: id, Count: searchResult.count);

            }
            else if (typeKey == Material)
            {
                var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(indexes, filter);
                int count = await GetCountOfParentMaterials(searchResult);
                return (Id: id, Count: count);
            }
            else
            {
                var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(indexes,filter);
                return (Id: id, Count: searchResult.Count);
            }
        }

        private async Task<int> GetCountOfParentMaterials(SearchByConfiguredFieldsResult searchResult)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                var foundMaterialIds = searchResult.Items.Keys.ToArray();
                var count = await _context.Materials.CountAsync(p => p.ParentId == null && foundMaterialIds.Contains(p.Id));
                return count;
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        private string[] GetOntologyIndexes(IOntologyModel ontology, string typeName)
        {
            var types = ontology.EntityTypes.Where(p => p.Name == typeName);

            return types.SelectMany(e => ontology.GetChildTypes(e))
                                        .Concat(types)
                                        .Distinct()
                                        .Select(nt => nt.Name)
                                        .ToArray();
        }
    }
}