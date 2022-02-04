using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;

namespace Iis.MaterialDistributor.MediatR.EventHandlers
{
    public class GetNextAssignedMaterialRequestHandler : IRequestHandler<GetNextAssignedMaterial, Guid?>
    {
        public Task<Guid?> Handle(GetNextAssignedMaterial request, CancellationToken cancellationToken)
        {
            //тут должен быть вызов из кеша\очереди которая under-construction
            return Task.FromResult<Guid?>(request.UserId);
        }
    }
}