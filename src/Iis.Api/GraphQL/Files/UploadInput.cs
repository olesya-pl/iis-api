using Iis.Interfaces.Enums;

namespace IIS.Core.GraphQL.Files
{
    public class UploadInput
    {
        public byte[] Content { get; set; }
        public string Name { get; set; }
        public AccessLevel AccessLevel { get; set; }
    }
}
