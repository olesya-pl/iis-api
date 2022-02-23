using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Ontology
{
    public class GetEntityOptions
    {
        public bool DummyIfNoAccess { get; set; }
        public bool NullValues { get; set; }
    }
}
