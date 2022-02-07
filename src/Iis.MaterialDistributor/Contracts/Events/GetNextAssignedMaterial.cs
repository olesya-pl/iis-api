using System;
using MediatR;

namespace Iis.MaterialDistributor.Contracts.Events
{
    public class GetNextAssignedMaterial : IRequest<Guid?>
    {
        public Guid UserId { get; set; }
    }
}