using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class UserDistributionItem
    {
        public Guid Id { get; }
        public IReadOnlyList<string> RoleNames { get; }
        public IReadOnlyList<string> Channels { get; }
        public string RolesText { get; }
        public int FreeSlots { get; set; }
        public UserDistributionItem() { }
        public UserDistributionItem(Guid id, int freeSlots, 
            IEnumerable<string> roleNames,
            IEnumerable<string> channels = null)
        {
            Id = id;
            FreeSlots = freeSlots;
            RoleNames = roleNames?.ToList() ?? new List<string>();
            RolesText = RoleNames.Count == 0 ? null : string.Join(',', RoleNames);
            Channels = channels?.ToList() ?? new List<string>();
        }

        public override string ToString() =>
            $"Id: {Id}, FreeSlots: {FreeSlots}, Roles: {RolesText}";
    }
}
