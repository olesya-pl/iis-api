using System;

namespace Iis.RabbitMq.Channels
{
    public interface IPublishMessageChannel<T> : IDisposable
    {
        void Send(T message);
        void Send(T message, params string[] routingKeyList);
    }
}