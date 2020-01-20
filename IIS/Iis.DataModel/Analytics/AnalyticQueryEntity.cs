using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Iis.DataModel.Analytics
{
    public class AnalyticQueryEntity
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

        public List<DateRange> DateRanges { get; set; } = new List<DateRange>();

        public virtual UserEntity Creator { get; set; }

        public virtual UserEntity LastUpdater { get; set; }

        public virtual ICollection<AnalyticQueryIndicatorEntity> Indicators { get; set; } = new List<AnalyticQueryIndicatorEntity>();

        public class DateRange {
            public int Id { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Color { get; set; }
        }
    }

}
