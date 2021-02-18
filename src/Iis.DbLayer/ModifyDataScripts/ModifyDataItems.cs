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
        
        public ModifyDataItems Add(ModifyDataItem item)
        {
            if (string.IsNullOrEmpty(item.Name))
                throw new Exception("Name of MOdifyDataItem cannot be null");

            if (Items.Any(item => item.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new Exception($"Name {item.Name} already exists");

            _items.Add(item);

            return this;
        }
        public ModifyDataItems Add(string name, ModifyDataAction action)
        {
            return Add(new ModifyDataItem(name, action));
        }
    }
}
