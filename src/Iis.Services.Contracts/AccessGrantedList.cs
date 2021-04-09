using System.Collections.Generic;
using System.Linq;
using Iis.Interfaces.Roles;

namespace Iis.Services.Contracts
{
    public class AccessGrantedList: List<AccessGranted>
    {
        public AccessGrantedList() { }

        public AccessGrantedList(IEnumerable<AccessGranted> accesses)
        {
            Merge(accesses);
        }

        public AccessGrantedList Merge(IEnumerable<AccessGranted> otherList)
        {
            foreach (var otherItem in otherList)
            {
                var localItem = this.SingleOrDefault(ag => ag.Kind == otherItem.Kind && ag.Category == otherItem.Category);
                if (localItem == null)
                {
                    this.Add(otherItem);
                }
                else
                {
                    localItem.MergeGranted(otherItem);
                }
            }
            return this;
        }

        public bool IsGranted(AccessKind kind, AccessOperation operation)
        {
            var item = this.SingleOrDefault(ag => ag.Kind == kind);
            return item == null ? false : item.IsGranted(operation);
        }
    }
}
