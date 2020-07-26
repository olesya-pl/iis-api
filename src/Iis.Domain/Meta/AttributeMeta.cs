using Iis.Interfaces.Meta;

namespace Iis.Domain.Meta
{
    public class AttributeMeta : IMeta
    {
        public IValidation Validation { get; set; }
        public SearchType? Kind { get; set; }
    }
}