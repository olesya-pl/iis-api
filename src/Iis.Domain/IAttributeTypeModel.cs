using Iis.Interfaces.Ontology.Schema;
using System;

namespace Iis.Domain
{
    public interface IAttributeTypeModel: INodeTypeModel
    {
        ScalarType ScalarTypeEnum { get; }

        bool AcceptsScalar(object value);
    }
}