using System;
using System.Threading.Tasks;

namespace Iis.RabbitMq.Channels
{
    public interface IRPCConsumeMessageChannel<TRequest, TResponse> : IDisposable
    {
        void SetOnMessageReceived(Func<TRequest, Task<TResponse>> function);
    }
}