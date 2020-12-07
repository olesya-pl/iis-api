using Iis.Events.Entities;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.EventHandlers
{
    public class EntityElasticEventHandler :
        INotificationHandler<EntityCreatedEvent>,
        INotificationHandler<EntityUpdatedEvent>
    {
        private readonly IElasticService _elasticService;
        private readonly IElasticState _elasticState;

        public EntityElasticEventHandler(IElasticService elasticService, IElasticState elasticState)
        {
            _elasticService = elasticService ?? throw new ArgumentNullException(nameof(elasticService));
            _elasticState = elasticState ?? throw new ArgumentNullException(nameof(elasticState));
        }

        public Task Handle(EntityCreatedEvent notification, CancellationToken cancellationToken)
        {
            if (_elasticState.FieldsToExcludeByIndex.TryGetValue(notification.Type, out var fieldsToExclude))
            {
                return _elasticService.PutNodeAsync(notification.Id, fieldsToExclude, cancellationToken);
            }
            else
            {
                return _elasticService.PutNodeAsync(notification.Id, cancellationToken);
            }

            
        }

        public Task Handle(EntityUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            if (_elasticState.FieldsToExcludeByIndex.TryGetValue(notification.Type, out var fieldsToExclude))
            {
                tasks.Add(_elasticService.PutNodeAsync(notification.Id, fieldsToExclude, cancellationToken));
            }
            else 
            {
                tasks.Add(_elasticService.PutNodeAsync(notification.Id, cancellationToken));
            }
                
            if (!string.Equals(notification.Type, "Event", StringComparison.OrdinalIgnoreCase))
            {
                tasks.Add(_elasticService.PutHistoricalNodesAsync(notification.Id, notification.RequestId));
            }

            return Task.WhenAll(tasks);
        }
    }
}
