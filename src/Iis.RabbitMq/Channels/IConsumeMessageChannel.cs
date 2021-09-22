using System;
using System.Threading.Tasks;

namespace Iis.RabbitMq.Channels
{
    public interface IConsumeMessageChannel<T> : IDisposable
    {
        Func<T, Task> OnMessageReceived { set; }
    }
}