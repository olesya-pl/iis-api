using Iis.Interfaces.Ontology.Data;

namespace Iis.OntologyData.DataTypes
{
    public class OntologyPatch : IOntologyPatch
    {
        internal OntologyPatchItem _create = new OntologyPatchItem();
        public IOntologyPatchItem Create => _create;

        internal OntologyPatchItem _update = new OntologyPatchItem();
        public IOntologyPatchItem Update => _update;

        public void AddAsCreated(NodeData node) => _create.Add(node);
        public void AddAsCreated(RelationData relation) => _create.Add(relation);
        public void AddAsCreated(AttributeData attribute) => _create.Add(attribute);
        public void AddAsUpdated(NodeData node)
        {
            if (_create.NodeExists(node.Id))
                _create.Add(node);
            else
                _update.Add(node);
        }
        public void AddAsUpdated(RelationData relation)
        {
            if (_create.RelationExists(relation.Id))
                _create.Add(relation);
            else
                _update.Add(relation);
        }
        public void AddAsUpdated(AttributeData attribute)
        {
            if (_create.AttributeExists(attribute.Id))
                _create.Add(attribute);
            else
                _update.Add(attribute);
        }

        public void Clear()
        {
            _create.Clear();
            _update.Clear();
        }
    }
}
