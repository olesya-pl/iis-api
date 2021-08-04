using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Common
{
    public class IdTitleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string NodeTypeName { get; set; }
    }
}
