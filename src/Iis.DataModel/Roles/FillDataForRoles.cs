using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
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
            _context.AccessObjects.AddRange(new List<AccessObjectEntity>
            {
                new AccessObjectEntity
                {
                    Id = new Guid("01380557fb27480c96ed6c56b8ae45a8"),
                    Title = "Об'єкти розвідки",
                    Kind = AccessKind.Dor,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    DeleteAllowed = true
                },
                new AccessObjectEntity
                {
                    Id = new Guid("02c1895f7d444512a0a97ebbf6c6690c"),
                    Title = "Матеріали",
                    Kind = AccessKind.Material,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    DeleteAllowed = true
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
                    DeleteAllowed = true
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
                    Id = new Guid("06be568c17aa4c38983aae5e80dac279"),
                    Title = "Події",
                    Kind = AccessKind.EventsTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("076b6fd6204b46d7afc923b3328687a4"),
                    Title = "Об'єкти розвідки",
                    Kind = AccessKind.DorTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                },
                new AccessObjectEntity
                {
                    Id = new Guid("08e273695e9a49ee8eb4daa305cf9029"),
                    Title = "Вхідний поток",
                    Kind = AccessKind.MaterialsTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                }
            });
        }
    }
}
