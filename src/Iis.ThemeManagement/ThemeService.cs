using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Newtonsoft.Json.Linq;

using IIS.Domain;
using Iis.Domain;
using Iis.DataModel;
using Iis.DataModel.Themes;
using Iis.ThemeManagement.Models;

namespace Iis.ThemeManagement
{
    public class ThemeService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IOntologyProvider _ontologyProvider;

        private IOntologyService _ontologyService;

        public ThemeService(OntologyContext context, IMapper mapper,IOntologyProvider ontologyProvider)
        {
            _context = context;
            _mapper = mapper;
            _ontologyProvider = ontologyProvider;
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

            var searchTasks = entities.Select(async e => {

                                    var filter = new ElasticFilter
                                    {
                                        Limit = 1000,
                                        Offset = 0,
                                        Suggestion = e.Query
                                    };

                                    var searchFunc = GetSearchFunction(e.Type.ShortTitle);

                                    if(searchFunc is null) return (id:e.Id, count: 0);

                                    var searchResult = await searchFunc(filter, default(CancellationToken));

                                    return (id: e.Id, count: searchResult.count);
                                });
            var searchResults = await Task.WhenAll(searchTasks);
            
            var themes = _mapper.Map<IEnumerable<Theme>>(entities);

            themes = themes.Join(searchResults,
                                e => e.Id,
                                r => r.id,
                                (e, r) => {
                                    e.QueryResults = r.count;
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

        private Func<ElasticFilter, CancellationToken, Task<(IEnumerable<JObject> nodes, int count)>> GetSearchFunction(string key)
        {
            return key switch 
            {
                "М" => _ontologyService.FilterEventsAsync,
                "О" => _ontologyService.FilterObjectsOfStudyAsync,
                "П" => _ontologyService.FilterEventsAsync,
                _ => null
            };
        }
    }
}