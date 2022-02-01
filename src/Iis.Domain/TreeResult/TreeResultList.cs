using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Domain.TreeResult
{
    public class TreeResultList
    {
        public List<TreeResult> Items { get; private set; }

        public TreeResultList Init<T>(
            IEnumerable<T> items,
            Func<T, string> labelFunc,
            Func<T, string> valueFunc,
            Func<T, string> sectionFunc)
        {
            Items = new List<TreeResult>();
            foreach (var item in items.OrderBy(x => sectionFunc(x)).ThenBy(x => labelFunc(x)))
            {
                var sectionName = sectionFunc(item);
                var section = Items.Where(r => r.Label == sectionName).SingleOrDefault();
                if (section == null)
                {
                    section = new TreeResult { Label = sectionName };
                    Items.Add(section);
                }
                section.Options.Add(new TreeResult { Label = labelFunc(item), Value = valueFunc(item) });
            }
            return this;
        }

        public TreeResultList Init<T>(
            IEnumerable<T> items,
            Func<T, string> labelFunc,
            Func<T, string> valueFunc,
            Func<T, IReadOnlyList<T>> optionsFunc)
        {
            Items = items
                .Select(_ => new TreeResult().Init(_, labelFunc, valueFunc, optionsFunc))
                .ToList();
            return this;
        }

        public string GetJson(string labelName, string valueName, string optionsName)
        {
            var jItems = new JArray();
            foreach (var item in Items)
            {
                jItems.Add(item.GetJsonObject(labelName, valueName, optionsName));
            }
            return JsonConvert.SerializeObject(jItems);
        }

    }
}
