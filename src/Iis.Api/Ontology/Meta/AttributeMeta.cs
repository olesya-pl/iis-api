using Iis.Domain.Meta;

namespace IIS.Core.Ontology.Meta
{
    public class AttributeMeta : IMeta
    {
        public IValidation Validation { get; set; }
        public SearchType? Kind { get; set; }
    }
    
    public enum SearchType
    {
        Keyword
    }
}