using System;
using Iis.Messages;
using Iis.Messages.Materials;

namespace Iis.MaterialLoader.Rabbit
{
    public interface IMaterialEventProducer : IDisposable
    {
        void PublishMaterialCreatedMessage(MaterialCreatedMessage message);
    }
}