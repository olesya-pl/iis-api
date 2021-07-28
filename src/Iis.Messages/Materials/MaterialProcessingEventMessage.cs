using System;
namespace Iis.Messages.Materials
{
    /// <summary>
    /// generic Message type for publish Material
    /// </summary>
    public class MaterialProcessingEventMessage
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
    }
}