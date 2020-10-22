using Iis.Interfaces.Ontology.Schema;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    internal interface IFormatAttributeService
    {
        object FormatValue(ScalarType scalarType, string value);
    }
}