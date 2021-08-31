using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class DistributionResultItem
    {
        public Guid MaterialId { get; }
        public Guid UserId { get; }
        public DistributionResultItem(Guid materialId, Guid userId)
        {
            MaterialId = materialId;
            UserId = userId;
        }
        public bool IsEqual(DistributionResultItem item) =>
            MaterialId == item.MaterialId &&
            UserId == item.UserId;

        public override string ToString() =>
            $"MaterialId: {MaterialId}; UserId: {UserId}";
    }
}
