using Iis.Events.Entities;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.EventHandlers
{
    public class EntityElasticEventHandler :
        INotificationHandler<EntityCreatedEvent>,
        INotificationHandler<EntityUpdatedEvent>,
        INotificationHandler<EntityDeleteEvent>
    {
        private readonly IElasticService _elasticService;
        private readonly IElasticState _elasticState;
        private readonly ISecurityLevelService _securityLevelService;

        public EntityElasticEventHandler(
            IElasticService elasticService,
            IElasticState elasticState,
            ISecurityLevelService securityLevelService)
        {
            _elasticService = elasticService;
            _elasticState = elasticState;
            _securityLevelService = securityLevelService;
        }

        public Task Handle(EntityCreatedEvent notification, CancellationToken cancellationToken)
        {
            return PutActualNodeAsync(notification, cancellationToken);
        }

        public async Task Handle(EntityUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await PutActualNodeAsync(notification, cancellationToken);
            if (notification.SecurityLevelChanged)
            {
                var changedNodes = _securityLevelService.ChangeSecurityLevelsForLinkedNodes(notification.Id);
                await _elasticService.PutNodesAsync(changedNodes, cancellationToken);
            }
        }

        public Task Handle(EntityDeleteEvent notification, CancellationToken cancellationToken)
        {
            return _elasticService.DeleteNodeAsync(notification.Id, notification.Type, cancellationToken);
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