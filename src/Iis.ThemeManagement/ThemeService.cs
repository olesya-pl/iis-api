using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Iis.DataModel;
using Iis.DataModel.Themes;
using Iis.ThemeManagement.Models;

namespace Iis.ThemeManagement
{
    public class ThemeService
    {
        private OntologyContext _context;
        private IMapper _mapper;

        public ThemeService(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            return await Task.FromResult<IEnumerable<Theme>>(null);
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
    }
}