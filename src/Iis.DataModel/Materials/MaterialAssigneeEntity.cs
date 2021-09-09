using System;

namespace Iis.DataModel.Materials
{
    public class MaterialAssigneeEntity
    {
        public Guid MaterialId { get; set; }
        public Guid AssigneeId { get; set; }
        public MaterialEntity Material { get; set; }
        public UserEntity Assignee { get; set; }

        public static MaterialAssigneeEntity CreateFrom(Guid materialId, Guid assigneeId) =>
            new MaterialAssigneeEntity
            {
                MaterialId = materialId,
                AssigneeId = assigneeId
            };
    }
}