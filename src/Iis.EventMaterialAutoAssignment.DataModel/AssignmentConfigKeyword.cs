using System;

namespace Iis.EventMaterialAutoAssignment
{
    public class AssignmentConfigKeyword
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public Guid AssignmentConfigId { get; set; }

        public AssignmentConfig AssignmentConfig { get; set; }
    }
}