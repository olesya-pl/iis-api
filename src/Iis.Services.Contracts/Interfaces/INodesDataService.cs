﻿using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IOntologyDataService
    {
        void ReloadOntologyData(string connectionString);
    }
}