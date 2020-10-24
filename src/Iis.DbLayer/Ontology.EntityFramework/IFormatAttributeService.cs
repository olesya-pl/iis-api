using System;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public interface IFormatAttributeService
    {
        object FormatValue(ScalarType scalarType, string value);
        object FormatRange(string value);
    }
}