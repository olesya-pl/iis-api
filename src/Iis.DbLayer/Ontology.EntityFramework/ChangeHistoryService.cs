using Iis.DataModel;
using Iis.DataModel.ChangeHistory;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class ChangeHistoryService : IChangeHistoryService
    {
        OntologyContext _context;
        IOntologySchema _schema;
        public ChangeHistoryService(OntologyContext context, IOntologySchema schema)
        {
            _context = context;
            _schema = schema;
        }

        public async Task SaveChange(string attributeDotName, Guid targetId, string userName, string oldValue, string newValue)
        {
            var changeHistoryEntity = new ChangeHistoryEntity
            {
                Id = Guid.NewGuid(),
                TargetId = targetId,
                UserName = userName, 
                PropertyName = attributeDotName,
                Date = DateTime.Now,
                OldValue = oldValue,
                NewValue = newValue
            };
            _context.Add(changeHistoryEntity);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChange(Guid typeId, Guid rootTypeId, Guid targetId, string userName, string oldValue, string newValue)
        {
            var attributeDotName = _schema.GetAttributeTypeDotName(typeId, rootTypeId);
            await SaveChange(attributeDotName, targetId, userName, oldValue, newValue);
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
    }
}
