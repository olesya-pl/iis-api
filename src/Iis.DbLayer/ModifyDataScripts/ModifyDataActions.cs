using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
