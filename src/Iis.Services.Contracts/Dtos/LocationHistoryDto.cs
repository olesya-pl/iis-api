using System;

namespace Iis.Services.Contracts.Dtos
{
    public class LocationHistoryDto
    {
        public Guid Id { get; set; }
        public decimal Lat { get; set; }
        public decimal Long { get; set; }
        public DateTime RegisteredAt { get; set; }
        public Guid? NodeId { get; set; }
        public Guid? EntityId { get; set; }
        public Guid? MaterialId { get; set; }
    }
}