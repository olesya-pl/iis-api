using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            

            return await Task.FromResult(theme.Id);
        }

        public async Task<Theme> DeleteThemeAsync(Guid themeId)
        {
           var userEntity = new UserEntity
           {
               Id = Guid.NewGuid(),
               Username = "testUser"
           };

            var typeEntity = new ThemeTypeEntity
            {
                Id = new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20"),
                ShortTitle = "М",
                Title = "Матеріал",
                EntityTypeName = "material"
            };

            var entity = new ThemeEntity
            {
                Id = themeId,
                Query = "query field",
                Title = "title field",
                TypeId = typeEntity.Id,
                Type = typeEntity,
                UserId = userEntity.Id,
                User = userEntity
            };

            return await Task.FromResult(_mapper.Map<Theme>(entity));
        }
        
        public async Task<Theme> GetThemeAsync(Guid themeId)
        {
           var userEntity = new UserEntity
           {
               Id = Guid.NewGuid(),
               Username = "testUser"
           };

            var typeEntity = new ThemeTypeEntity
            {
                Id = new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20"),
                ShortTitle = "М",
                Title = "Матеріал",
                EntityTypeName = "material"
            };

            var entity = new ThemeEntity
            {
                Id = themeId,
                Query = "query field",
                Title = "title field",
                TypeId = typeEntity.Id,
                Type = typeEntity,
                UserId = userEntity.Id,
                User = userEntity
            };

            return await Task.FromResult(_mapper.Map<Theme>(entity));
        }

        public async Task<IEnumerable<Theme>> GetThemesByUserIdAsync(Guid userId)
        {
            return await Task.FromResult<IEnumerable<Theme>>(null);
        }

        public async Task<ThemeType> GetThemeTypeByEntityTypeNameAsync(string entityTypeName)
        {
            var entity = new ThemeTypeEntity
            {
                Id = new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20"),
                ShortTitle = "М",
                Title = "Матеріал",
                EntityTypeName = "material"
            };

            return await Task.FromResult(_mapper.Map<ThemeType>(entity));
        }
    }
}