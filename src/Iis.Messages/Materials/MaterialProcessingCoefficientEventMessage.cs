using System;

namespace Iis.Messages.Materials
{
    public class MaterialProcessingCoefficientEventMessage
    {
        public Material[] Materials { get; set; }
    }

    public class Material
    {
        public Guid Id { get; set; }
        public string Metadata { get; set; }
        public RelatedObjectOfStudy[] RelatedObjectCollection { get; set; }
    }

    public class RelatedObjectOfStudy
    {
        public Guid Id { get; set; }
        public Importance? Importance { get; set; }
    }
}