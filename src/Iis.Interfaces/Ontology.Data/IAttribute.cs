using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IAttribute: IAttributeBase
    {
        INode Node { get; }
    }
}
