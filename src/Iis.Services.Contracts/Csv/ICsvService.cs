using Iis.Interfaces.Ontology.Data;
using System.Collections.Generic;

namespace Iis.Services.Contracts.Csv
{
    public interface ICsvService
    {
        string GetDorCsv(IEnumerable<INode> entities);
        string GetDorCsvByTypeName(string typeName);
    }
}