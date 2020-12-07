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
            _elasticService = elasticService;
            _elasticState = elasticState;
        }

        public Task Handle(EntityCreatedEvent notification, CancellationToken cancellationToken)
        {
            return PutActualNodeAsync(notification, cancellationToken);
        }

        public Task Handle(EntityUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            tasks.Add(PutActualNodeAsync(notification, cancellationToken));
                
            if (!string.Equals(notification.Type, "Event", StringComparison.OrdinalIgnoreCase))
            {
                tasks.Add(_elasticService.PutHistoricalNodesAsync(notification.Id, notification.RequestId, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }
        
        private Task PutActualNodeAsync(EntityEvent notification, CancellationToken cancellationToken)
        {
            if (_elasticState.FieldsToExcludeByIndex.TryGetValue(notification.Type, out var fieldsToExclude))
            {
                return _elasticService.PutNodeAsync(notification.Id, fieldsToExclude, cancellationToken);
            }

            return _elasticService.PutNodeAsync(notification.Id, cancellationToken);
        }
    }
}
