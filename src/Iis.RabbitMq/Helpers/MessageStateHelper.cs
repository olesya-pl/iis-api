using RabbitMQ.Client.Events;
using Iis.RabbitMq.Channels;

namespace Iis.RabbitMq.Helpers
{
    public static class MessageStateHelper
    {
        public static MessageState ToState(this BasicDeliverEventArgs args)
        {
            return new MessageState
            {
                ExchangeName = args.Exchange,
                RoutingKey = args.RoutingKey,
                Body = args.Body.ToText()
            };
        }
    }
}