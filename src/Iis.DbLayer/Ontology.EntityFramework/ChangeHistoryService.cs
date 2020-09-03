using Iis.DataModel;
using Iis.DataModel.ChangeHistory;
using Iis.Interfaces.Ontology;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class ChangeHistoryService : IChangeHistoryService
    {
        OntologyContext _context;
        public ChangeHistoryService(OntologyContext context)
        {
            _context = context;
        }

        public async Task SaveChange(
            string attributeDotName,
            Guid targetId,
            string userName,
            string oldValue,
            string newValue,
            Guid requestId)
        {
            var changeHistoryEntity = new ChangeHistoryEntity
            {
                Id = Guid.NewGuid(),
                TargetId = targetId,
                UserName = userName,
                PropertyName = attributeDotName,
                Date = DateTime.Now,
                OldValue = oldValue,
                NewValue = newValue,
                RequestId = requestId
            };
            _context.Add(changeHistoryEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<IChangeHistoryItem>> GetChangeHistory(Guid targetId, string propertyName)
        {
            var query = _context.ChangeHistory.Where(ch => ch.TargetId == targetId);
            if (!string.IsNullOrEmpty(propertyName))
            {
                query = query.Where(ch => ch.PropertyName == propertyName);
            }
            return await query.OrderByDescending(ch => ch.Date).ToListAsync();
        }

        public async Task<IReadOnlyList<IChangeHistoryItem>> GetChangeHistoryByRequest(Guid requestId) 
        {
            return await _context.ChangeHistory.AsNoTracking().Where(ch => ch.RequestId == requestId).ToListAsync();
        }
    }
}
