using System.Collections.Generic;

namespace IIS.Core
{
    public interface IOntologyNode
    {
        void AcceptVisitor(IOntologyVisitor visitor);
        ISchemaNode Schema { get; }
        IEnumerable<IOntologyNode> Nodes { get; }
    }
}
