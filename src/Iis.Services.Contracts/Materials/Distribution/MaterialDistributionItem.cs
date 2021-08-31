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
        public MaterialDistributionItem() { }
        public MaterialDistributionItem(Guid id, int priority, string roleName, string channel)
        {
            Id = id;
            Priority = priority;
            RoleName = roleName;
            Channel = channel;
        }

        public override string ToString() =>
            $"Priority: {Priority}; Role: {RoleName}; Channel: {Channel}; Id: {Id}";
    }
}
