using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IMigrationEntity
    {
        string SourceEntityName { get; }
        string TargetEntityName { get; }
        List<string> LinkedEntities { get; }
        string GetTargetDotName(string sourceDotName);
        IMigrationItem GetItem(string sourceDotName);
    }
}
