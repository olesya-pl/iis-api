using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaAliases : IAliases
    {
        private Dictionary<string, SchemaAlias> _dict = new Dictionary<string, SchemaAlias>();
        public IEnumerable<IAlias> Items => _dict.Values;

        public SchemaAliases(IEnumerable<IAlias> source)
        {
            _dict = source.ToDictionary(a => a.DotName, a => new SchemaAlias(a));
        }

        public IAlias GetItem(string dotName)
        {
            return _dict.ContainsKey(dotName) ? _dict[dotName] : null;
        }

        public IEnumerable<string> GetKeys(string entityName = null)
        {
            return _dict.Keys.Where(key => string.IsNullOrEmpty(entityName) || key.StartsWith(entityName));
        }
        public List<string> GetStrings(string entityName = null)
        {
            return GetKeys(entityName)
                .Select(key => $"{GetShortName(key)}:{_dict[key].Value}")
                .ToList();
        }
        public void Update(string entityName, IEnumerable<string> semiAliases)
        {
            var newDict = GetDictionary(entityName, semiAliases);

            foreach (var key in GetKeys(entityName))
            {
                if (!newDict.ContainsKey(key))
                {
                    _dict.Remove(key);
                }
            }

            foreach (var key in newDict.Keys)
            {
                if (_dict.ContainsKey(key))
                {
                    _dict[key].Value = newDict[key];
                }
                else
                {
                    var item = new SchemaAlias(key, newDict[key]);
                    _dict.Add(key, item);
                }
            }
        }

        public (IEnumerable<IAlias> itemsToAdd, IEnumerable<IAlias> itemsToUpdate, IEnumerable<IAlias> itemsToDelete) CompareTo(IAliases other)
        {
            return (
                _dict.Keys.Where(key => !other.Exists(key)).Select(key => _dict[key]),
                _dict.Keys.Where(key => other.Exists(key) && other.GetItem(key)?.Value != _dict[key].Value).Select(key => _dict[key]),
                other.Items.Where(item => !Exists(item.DotName))
            );
        }

        public bool Exists(string key)
        {
            return _dict.ContainsKey(key);
        }

        private Dictionary<string, string> GetDictionary(string nodeTypeName, IEnumerable<string> semiAliases)
        {
            var result = new Dictionary<string, string>();
            if (semiAliases == null) return result;
            foreach (var semiAlias in semiAliases)
            {
                var n = semiAlias.IndexOf(':');
                var shortName = semiAlias.Substring(0, n);
                var alias = semiAlias.Substring(n + 1);
                var fullName = $"{nodeTypeName}.{shortName}";
                result.Add(fullName, alias);
            }
            return result;
        }
        private string GetShortName(string fullName)
        {
            return fullName.Substring(fullName.IndexOf('.') + 1);
        }
    }
}
