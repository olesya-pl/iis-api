using Iis.Interfaces.Enums;
using System;

namespace Iis.Services.Contracts.Dtos
{
    public class AliasDto
    {
        public Guid Id { get; set; }
        public string DotName { get; set; }
        public string Value { get; set; }
        public AliasType Type { get; set; }
    }
}
