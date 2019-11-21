using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IIS.Core.Users.EntityFramework;

namespace IIS.Core.Analytics.EntityFramework
{
    public class AnalyticsQuery
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        public Guid LastUpdaterId { get; set; }

        [Required]
        public Guid RootIndicatorId { get; set; }

        public virtual User Creator { get; set; }

        public virtual User LastUpdater { get; set; }

        public virtual AnalyticsIndicator RootIndicator { get; set; }

        public virtual ICollection<AnalyticsQueryIndicator> Indicators { get; set; } = new List<AnalyticsQueryIndicator>();
    }
}
