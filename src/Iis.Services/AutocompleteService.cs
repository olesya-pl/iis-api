using System;
using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Services
{
    public class AutocompleteService : IAutocompleteService
    {
        private readonly IOntologySchema _ontologySchema;
        private static readonly List<string> KeyWords = new List<string>();

        public AutocompleteService(IOntologySchema ontologySchema)
        {
            _ontologySchema = ontologySchema;
        }

        public List<string> GetTips(string query, int count)
        {
            var result = new List<string>(count);
            result.AddRange(GetKeyWords().Where(x => x.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)));
            
            if (result.Count < count)
            {
                result.AddRange(GetKeyWords().Where(x => x.Contains(query, StringComparison.InvariantCultureIgnoreCase)));
            }

            return result
                .Distinct()
                .Take(count)
                .ToList();
        }

        private List<string> GetKeyWords()
        {
            if (!KeyWords.Any())
            {
                var aliases = _ontologySchema.Aliases.Items.Select(x => x.Value).ToList();
                var fields = _ontologySchema.GetFullHierarchyNodes()
                    .Where(x => x.Value.Kind == Kind.Attribute)
                    .Select(x => x.Key.Substring(x.Key.IndexOf('.') + 1))
                    .Distinct()
                    .ToList();

                KeyWords.AddRange(aliases);
                KeyWords.AddRange(fields);
            }

            return KeyWords;
        }
    }
}
