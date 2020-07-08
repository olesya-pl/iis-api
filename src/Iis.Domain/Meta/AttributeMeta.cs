using Iis.Interfaces.Meta;

namespace Iis.Domain.Meta
{
    public class AttributeMeta : IAttributeMeta
    {
        public IValidation Validation { get; set; }
        public SearchType? Kind { get; set; }
    }
}