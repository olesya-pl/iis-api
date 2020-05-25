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

namespace Iis.ThemeManagement
{
    public class ThemeService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly IElasticService _elasticService;

        public ThemeService(OntologyContext context, IMapper mapper,IOntologyProvider ontologyProvider, IElasticService elasticService)
        {
            _context = context;
            _mapper = mapper;
            _ontologyProvider = ontologyProvider;
            _elasticService = elasticService;
        }

        public async Task<Guid> CreateThemeAsync(Theme theme)
        {
            var entity = _mapper.Map<ThemeEntity>(theme);
            
            _context.Themes.Add(entity);

            await _context.SaveChangesAsync();

            return entity.Id;
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

            var ontology = await _ontologyProvider.GetOntologyAsync(default(CancellationToken));

            var searchTasks = entities.Select(e => {
                return ExecuteThemeQuery(e.Id, e.Type.ShortTitle, e.Query, ontology);
            });
            
            var searchResults = await Task.WhenAll(searchTasks);
            
            var themes = _mapper.Map<IEnumerable<Theme>>(entities);

            themes = themes.Join(searchResults,
                                e => e.Id,
                                r => r.Id,
                                (e, r) => {
                                    e.QueryResults = r.Count;
                                    return e;
                                }).ToList();
            
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

        private async Task<(Guid Id, int Count)> ExecuteThemeQuery(Guid id, string typeKey, string query, OntologyModel ontology)
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

            } else
            {
                var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(indexes,filter);

                return (Id: id, Count: searchResult.Count);
            }
        }
        private string[] GetOntologyIndexes(OntologyModel ontology, string typeName)
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