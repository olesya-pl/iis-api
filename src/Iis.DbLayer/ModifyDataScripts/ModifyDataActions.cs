using System;
using System.Linq;
using System.Collections.Generic;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.DbLayer.ModifyDataScripts
{
    public class ModifyDataActions
    {
        public void RemoveEventWikiLinks(OntologyContext context, IOntologyNodesData data)
        {
            var list = new List<IRelation>();
            data.WriteLock(() =>
            {
                var events = data.GetEntitiesByTypeName(EntityTypeNames.Event.ToString());
                foreach (var ev in events)
                {
                    var relations = ev.OutgoingRelations
                        .Where(r => r.Node.NodeType.Name == "associatedWithEvent"
                            && !r.TargetNode.NodeType.IsObjectOfStudy)
                        .ToList();

                    foreach (var relation in relations)
                    {
                        list.Add(relation);
                        data.RemoveNode(relation.Id);
                    }
                }
            });
        }

        public void AddAccessLevelAccessObject(OntologyContext context, IOntologyNodesData data)
        {
            var entityId = new Guid("a60af6c5d930476c96218ea5c0147fb7");

            var existingEntity = context.AccessObjects.Find(entityId);

            if( existingEntity != null) return;

            context.AccessObjects.Add(
                new AccessObjectEntity
                {
                    Id = entityId,
                    Title = "Зміна грифу обмеження доступу",
                    Kind = AccessKind.AccessLevelChange,
                    Category = AccessCategory.Entity,
                    UpdateAllowed = true
                }
            );
            context.SaveChanges();
        }
        
        public void AddAccessLevels(OntologyContext context, IOntologyNodesData data)
        {
            var enumEntityType = data.Schema.GetEntityTypeByName(EntityTypeNames.Enum.ToString());
            data.Schema.CreateEntityType("AccessLevels", "Access Levels", false, enumEntityType.Id);
        }
    }
}
