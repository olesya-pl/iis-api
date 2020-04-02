using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Roles
{
    public enum AccessKind
    {
        FreeForAll = 0,
        Dor = 1, 
        Material = 2, 
        Event = 3, 
        MapTab = 4,
        AdminTab = 5,
        EventsTab = 6,
        DorTab = 7,
        MaterialsTab = 8,
        MaterialDorLink = 9,
        EventLink = 10,
    }
}
