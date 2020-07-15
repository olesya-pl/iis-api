using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;

namespace Iis.OntologySchema.DataTypes
{
    public interface IAliases
    {
        IEnumerable<IAlias> Items { get; }
        (IEnumerable<IAlias> itemsToAdd, IEnumerable<IAlias> itemsToUpdate, IEnumerable<IAlias> itemsToDelete) CompareTo(IAliases other);
        bool Exists(string key);
        IAlias GetItem(string dotName);
        IEnumerable<string> GetKeys(string entityName = null);
        List<string> GetStrings(string entityName = null);
        void Update(string entityName, IEnumerable<string> semiAliases);
    }
}