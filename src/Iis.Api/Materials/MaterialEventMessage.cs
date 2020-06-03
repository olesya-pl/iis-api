using System;

namespace IIS.Core.Materials
{
    /// <summary>
    /// Message type for publish when Material is created|updated
    /// </summary>
    public class MaterialEventMessage
    {
        public Guid Id {get;set;}
        public string Source {get;set;}
        public string Type {get;set;}
    }
}
