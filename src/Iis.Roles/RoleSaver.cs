using Iis.DataModel;
using System;

namespace Iis.Roles
{
    public class RoleSaver
    {
        private OntologyContext _context;
        public RoleSaver(OntologyContext ontologyContext)
        {
            _context = ontologyContext;
        }


    }
}
