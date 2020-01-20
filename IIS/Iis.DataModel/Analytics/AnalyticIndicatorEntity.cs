using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Iis.DataModel.Analytics
{
    public class AnalyticIndicatorEntity
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Query { get; set; }

        public Guid? ParentId { get; set; }

        public virtual AnalyticIndicatorEntity Parent { get; set; }

        public virtual ICollection<AnalyticQueryIndicatorEntity> QueryIndicators { get; set; }

        public AnalyticIndicatorEntity(Guid id, string title)
        {
            Id = id;
            Title = title;
        }

        public AnalyticIndicatorEntity AddChild(AnalyticIndicatorEntity indicator)
        {
            indicator.Parent = this;
            indicator.ParentId = this.Id;
            return this;
        }
    }
}
