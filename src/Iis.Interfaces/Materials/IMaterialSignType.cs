using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Materials
{
    public interface IMaterialSignType
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Title { get; }
    }
}
