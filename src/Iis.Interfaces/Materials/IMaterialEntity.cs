using System;

namespace Iis.Interfaces.Materials
{
    public interface IMaterialEntity
    {
        Guid Id { get; set; }
        public string Metadata { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public string LoadData { get; set; }
    }
}
