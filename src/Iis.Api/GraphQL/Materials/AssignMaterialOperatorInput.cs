using System;

namespace IIS.Core.GraphQL.Materials
{
    public class AssignMaterialOperatorInput
    {
        public Guid MaterialId { get; set; }
        public Guid AssigneeId { get; set; }
    }
}
