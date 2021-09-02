using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionItem
    {
        public Guid Id { get; }
        public int Priority { get; }
        public string RoleName { get; }
        public string Channel { get; }
        public int AccessLevel { get; }
        public MaterialDistributionItem() { }
        public MaterialDistributionItem(Guid id, int priority, string roleName, string channel, int accessLevel = 0)
        {
            Id = id;
            Priority = priority;
            RoleName = roleName;
            Channel = channel;
            AccessLevel = accessLevel;
        }

        public MaterialDistributionItem(Guid id, string channel)
        {
            Id = id;
            Priority = 0;
            RoleName = null;
            Channel = channel;
        }

        public override string ToString() =>
            $"Priority: {Priority}; Role: {RoleName}; Channel: {Channel}; Id: {Id}";
    }
}
