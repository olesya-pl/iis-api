using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.DbLayer.ModifyDataScripts
{
    public class ModifyDataItems
    {
        private List<ModifyDataItem> _items = new List<ModifyDataItem>();
        public IReadOnlyList<ModifyDataItem> Items => _items;
        public ModifyDataItems Add(ModifyDataItem newItem)
        {
            if (string.IsNullOrEmpty(newItem.Name))
                throw new Exception("Name of ModifyDataItem cannot be null");

            if (Items.Any(item => item.Name.Equals(newItem.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new Exception($"Name {newItem.Name} already exists");

            _items.Add(newItem);

            return this;
        }
        public ModifyDataItems Add(string name, ModifyDataAction action)
        {
            return Add(new ModifyDataItem(name, action));
        }
    }
}
