using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Materials
{
    public interface IMaterialSign
    {
        public Guid Id { get; }
        Guid MaterialSignTypeId { get; }
        string ShortTitle { get; }
        string Title { get; }
        int OrderNumber { get; }
    }
}
