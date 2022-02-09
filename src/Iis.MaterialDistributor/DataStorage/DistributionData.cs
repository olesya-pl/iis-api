using System;
using System.Collections.Generic;
using System.Linq;
using Iis.MaterialDistributor.Contracts.DataStorage;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.DataStorage
{
    public class DistributionData : IDistributionData
    {
        private const int QueueSize = 200;
        private readonly IFinalRatingEvaluator _finalRatingEvaluator;
        private Dictionary<Guid, MaterialDistributionInfo> _materials;
        private Dictionary<Guid, List<MaterialDistributionInfo>> _userQueues = new Dictionary<Guid, List<MaterialDistributionInfo>>();
        private object _lock = new object();

        public DistributionData(
            IFinalRatingEvaluator finalRatingEvaluator)
        {
            _finalRatingEvaluator = finalRatingEvaluator;
        }

        public void RefreshMaterials(Dictionary<Guid, MaterialDistributionInfo> materials)
        {
            lock (_lock)
            {
                _materials = materials;
                _userQueues.Clear();
            }
        }

        public void Distribute(IReadOnlyList<UserDistributionInfo> users)
        {
            lock (_lock)
            {
                _userQueues.Clear();
                foreach (var user in users)
                {
                    _userQueues[user.Id] = GetUserQueue(user);
                }
            }
        }

        public MaterialDistributionInfo GetMaterialFromQueue(UserDistributionInfo user)
        {
            lock (_lock)
            {
                if (_userQueues.ContainsKey(user.Id))
                {
                    _userQueues[user.Id] = GetUserQueue(user);
                }

                if (_userQueues[user.Id].Count == 0) return null;

                var result = _userQueues[user.Id][0];
                _userQueues[user.Id].RemoveAt(0);
                return result;
            }
        }

        private List<MaterialDistributionInfo> GetUserQueue(UserDistributionInfo user)
        {
            foreach (var material in _materials.Values)
            {
                material.FinalRating = _finalRatingEvaluator.GetFinalRating(material, user);
            }

            var result = _materials.Values
                .Where(_ => _.FinalRating > 0)
                .OrderByDescending(_ => _.FinalRating)
                .Take(QueueSize)
                .ToList();

            return result;
        }
    }
}
