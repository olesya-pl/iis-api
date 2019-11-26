using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IIS.Core.Analytics.EntityFramework
{
    public class AnalyticsIndicator
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Query { get; set; }

        public Guid? ParentId { get; set; }

        public virtual AnalyticsIndicator Parent { get; set; }

        public virtual ICollection<AnalyticsQueryIndicator> QueryIndicators { get; set; }

        public AnalyticsIndicator(Guid id, string title)
        {
            Id = id;
            Title = title;
        }

        public AnalyticsIndicator AddChild(AnalyticsIndicator indicator)
        {
            indicator.Parent = this;
            indicator.ParentId = this.Id;
            return this;
        }
    }
}
