using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class FillDataForRoles
    {
        OntologyContext _context;
        public FillDataForRoles(OntologyContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            if (_context.AccessObjects.Any()) return;

            _context.AccessObjects.AddRange(new List<AccessObjectEntity>
            {
                new AccessObjectEntity
                {
                    Id = new Guid("01380557fb27480c96ed6c56b8ae45a8"),
                    Title = "Об'єкти розвідки",
                    Kind = AccessKind.Entity,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                },
                new AccessObjectEntity
                {
                    Id = new Guid("02c1895f7d444512a0a97ebbf6c6690c"),
                    Title = "Матеріали",
                    Kind = AccessKind.Material,
                    Category = AccessCategory.Entity,
                    CreateAllowed = false,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                },
                new AccessObjectEntity
                {
                    Id = new Guid("036137a67db34a0e9566f4ce9691a878"),
                    Title = "Події",
                    Kind = AccessKind.Event,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                },
                new AccessObjectEntity
                {
                    Id = new Guid("bb2fe99de99645528e89acc5bd7232e7"),
                    Title = "Звіти",
                    Kind = AccessKind.Report,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                },
                new AccessObjectEntity
                {
                    Id = new Guid("56c3dd7aeb8a424882ce82862c3c4388"),
                    Title = "Звіти",
                    Kind = AccessKind.Report,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("044b50afad56484489c460e167a5b52a"),
                    Title = "Мапа",
                    Kind = AccessKind.MapTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("05098de3f43a4157a48adc5090f13655"),
                    Title = "Адміністрування",
                    Kind = AccessKind.AdminTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("1d20fd240de84531a19c4986cb2d277b"),
                    Title = "Теми та оновлення",
                    Kind = AccessKind.ThemesTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("b51766b93422450ca165d9f9d98a1fb0"),
                    Title = "Завантаження матеріалів",
                    Kind = AccessKind.MaterialUpoadTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("cda32d549dd4403a94c391f8ff6d5bca"),
                    Title = "Довідник ОІВТ",
                    Kind = AccessKind.Wiki,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("06be568c17aa4c38983aae5e80dac279"),
                    Title = "Події",
                    Kind = AccessKind.Event,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("076b6fd6204b46d7afc923b3328687a4"),
                    Title = "Об'єкти розвідки",
                    Kind = AccessKind.Entity,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("08e273695e9a49ee8eb4daa305cf9029"),
                    Title = "Матеріали",
                    Kind = AccessKind.Material,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("0971390a21fa4ab4ae277bb4c7c5bd45"),
                    Title = "Прив'язка матеріалів до об'єктів розвідки",
                    Kind = AccessKind.MaterialDorLink,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("102617ecd2514b5f97e8be1a9bf99bc3"),
                    Title = "Прив'язка подій до об'єктів розвідки та матеріалів",
                    Kind = AccessKind.EventLink,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                },
            });

            var roles = new List<RoleEntity>
            {
                new RoleEntity { Id = new Guid("a120c2b8d6f84338ab0e5d177951f119"), Name = "Оператор", Description = "Редактує матеріали та прив'язуває їх до об'єктів розвідки" },
                new RoleEntity { Id = new Guid("a25012ad140643c08ab5ff3d682b7179"), Name = "Аналітик 1", Description = "Створений для первинної обробки матеріалів" },
                new RoleEntity { Id = new Guid("a3b1917a46be437689819c8c9f97ee19"), Name = "Аналітик 2", Description = "Володіе подіями" },
                new RoleEntity { Id = new Guid("a4826f2fdf4e42b6b1d89dffed5a5c13"), Name = "Адміністратор", Description = "Всемогутній", IsAdmin = true }
            };
            _context.Roles.AddRange(roles);

            _context.RoleAccess.AddRange(new List<RoleAccessEntity>
            {
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a120c2b8d6f84338ab0e5d177951f119"), AccessObjectId = new Guid("01380557fb27480c96ed6c56b8ae45a8"), 
                    ReadGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a120c2b8d6f84338ab0e5d177951f119"), AccessObjectId = new Guid("02c1895f7d444512a0a97ebbf6c6690c"),
                    ReadGranted = true, UpdateGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a120c2b8d6f84338ab0e5d177951f119"), AccessObjectId = new Guid("0971390a21fa4ab4ae277bb4c7c5bd45"),
                    ReadGranted = true, UpdateGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a120c2b8d6f84338ab0e5d177951f119"), AccessObjectId = new Guid("044b50afad56484489c460e167a5b52a"),
                    ReadGranted = true },

                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a25012ad140643c08ab5ff3d682b7179"), AccessObjectId = new Guid("01380557fb27480c96ed6c56b8ae45a8"),
                    ReadGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a25012ad140643c08ab5ff3d682b7179"), AccessObjectId = new Guid("02c1895f7d444512a0a97ebbf6c6690c"),
                    ReadGranted = true, UpdateGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a25012ad140643c08ab5ff3d682b7179"), AccessObjectId = new Guid("0971390a21fa4ab4ae277bb4c7c5bd45"),
                    ReadGranted = true, UpdateGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a25012ad140643c08ab5ff3d682b7179"), AccessObjectId = new Guid("036137a67db34a0e9566f4ce9691a878"),
                    ReadGranted = true, CreateGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a25012ad140643c08ab5ff3d682b7179"), AccessObjectId = new Guid("102617ecd2514b5f97e8be1a9bf99bc3"),
                    ReadGranted = true, CreateGranted = true },

                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a3b1917a46be437689819c8c9f97ee19"), AccessObjectId = new Guid("01380557fb27480c96ed6c56b8ae45a8"),
                    ReadGranted = true, CreateGranted = true, UpdateGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a3b1917a46be437689819c8c9f97ee19"), AccessObjectId = new Guid("02c1895f7d444512a0a97ebbf6c6690c"),
                    ReadGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a3b1917a46be437689819c8c9f97ee19"), AccessObjectId = new Guid("0971390a21fa4ab4ae277bb4c7c5bd45"),
                    ReadGranted = true, UpdateGranted = true },
                new RoleAccessEntity { Id = Guid.NewGuid(), RoleId = new Guid("a3b1917a46be437689819c8c9f97ee19"), AccessObjectId = new Guid("036137a67db34a0e9566f4ce9691a878"),
                    ReadGranted = true },
            });

            var tabIds = new List<string> { "06be568c17aa4c38983aae5e80dac279", "076b6fd6204b46d7afc923b3328687a4", "08e273695e9a49ee8eb4daa305cf9029" };
            foreach (var role in roles)
            {
                foreach (var tabId in tabIds)
                {
                    _context.RoleAccess.Add(new RoleAccessEntity
                    {
                        Id = Guid.NewGuid(),
                        RoleId = role.Id,
                        AccessObjectId = new Guid(tabId),
                        ReadGranted = true
                    });
                }
            }
            _context.SaveChanges();
        }
    }
}
