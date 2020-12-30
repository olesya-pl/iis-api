using Iis.Interfaces.Ontology.Data;

namespace Iis.OntologyData.DataTypes
{
    public class OntologyPatch : IOntologyPatch
    {
        internal OntologyPatchItem _create = new OntologyPatchItem();
        public IOntologyPatchItem Create => _create;

        internal OntologyPatchItem _update = new OntologyPatchItem();
        public IOntologyPatchItem Update => _update;

        public void Clear()
        {
            _create.Clear();
            _update.Clear();
        }
    }
}
