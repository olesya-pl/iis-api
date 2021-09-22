using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class DistributionResultItem
    {
        public Guid MaterialId { get; }
        public Guid UserId { get; }
        public string Channel { get; }
        public DistributionResultItem(Guid materialId, Guid userId, string channel = null)
        {
            MaterialId = materialId;
            UserId = userId;
            Channel = channel;
        }
        public bool IsEqual(DistributionResultItem item) =>
            MaterialId == item.MaterialId &&
            UserId == item.UserId;

        public override string ToString() =>
            $"MaterialId: {MaterialId}; UserId: {UserId}";
    }
}
