using System;
using Iis.Messages;

namespace Iis.MaterialLoader.Rabbit
{
    public interface IMaterialEventProducer : IDisposable
    {
        void PublishMaterialCreatedMessage(MaterialCreatedMessage message);
    }
}