using System;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.RabbitMq.Channels
{
    public interface IRPCPublishMessageChannel<TRequest, TResponse> : IDisposable
    {
        Task<TResponse> SendAsync(TRequest message, CancellationToken cancellationToken);
        Task<TResponse> SendAsync(TRequest message, string routingKey, CancellationToken cancellationToken);
    }
}