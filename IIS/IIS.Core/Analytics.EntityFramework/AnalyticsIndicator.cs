using System;
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

        [Required]
        [MaxLength(200)]
        public string Code { get; set; }

        public string Query { get; set; }

        public Guid? ParentId { get; set; }

        public virtual AnalyticsIndicator Parent { get; set; }

        public AnalyticsIndicator(Guid id, string title, string code)
        {
            Id = id;
            Title = title;
            Code = code;
        }

        public AnalyticsIndicator AddChild(AnalyticsIndicator indicator)
        {
            indicator.Parent = this;
            indicator.ParentId = this.Id;
            return this;
        }
    }
}
