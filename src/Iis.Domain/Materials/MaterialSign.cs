using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Materials
{
    public class MaterialSign
    {
        public Guid Id { get; set; }
        public Guid MaterialSignTypeId { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public int OrderNumber { get; set; }

    }
}
