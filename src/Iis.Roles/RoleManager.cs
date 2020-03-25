using Iis.DataModel;
using System;

namespace Iis.Roles
{
    public class RoleManager
    {
        private OntologyContext _context;
        private AccessObjects _accessObjects = new AccessObjects();
        public RoleManager(OntologyContext ontologyContext)
        {
            _context = ontologyContext;
        }


    }
}
