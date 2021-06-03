using Iis.Interfaces.Enums;

namespace IIS.Core.GraphQL.Files
{
    public class UploadInput
    {
        public string Name { get; set; }
        public int AccessLevel { get; set; }
    }
}
