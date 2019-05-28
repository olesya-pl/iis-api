using System.Collections.Generic;

namespace IIS.Core
{
    public interface ISchemaNode
    {
        void AcceptVisitor(ISchemaVisitor visitor);
        IEnumerable<ISchemaNode> Nodes { get; }
    }
}
