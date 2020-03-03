﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IOntologySchema
    {
        void Initialize(IOntologyRawData ontologyRawData);
        IEnumerable<INodeTypeLinked> GetTypes(IGetTypesFilter filter);
        INodeTypeLinked GetNodeTypeById(Guid id);
        IOntologyRawData GetRawData();
        void SetEmbeddingOptions(string entityName, string relationName, EmbeddingOptions embeddingOptions);
        void SetRelationMeta(string entityName, string relationName, string meta);
        ISchemaCompareResult CompareTo(IOntologySchema schema);
        Dictionary<string, INodeTypeLinked> GetStringCodes();
    }
}