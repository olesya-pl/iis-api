using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IOntologySchema: ISchemaEntityTypeFinder
    {
        IOntologySchemaSource SchemaSource { get; }
        void Initialize(IOntologyRawData ontologyRawData);
        IEnumerable<INodeTypeLinked> GetTypes(IGetTypesFilter filter);
        INodeTypeLinked GetNodeTypeById(Guid id);
        IOntologyRawData GetRawData();
        ISchemaCompareResult CompareTo(IOntologySchema schema);
        Dictionary<string, INodeTypeLinked> GetStringCodes();
        void UpdateNodeType(INodeTypeUpdateParameter updateParameter);
        void UpdateTargetType(Guid relationTypeId, Guid targetTypeId);
    }
}
