using System;

namespace Iis.MaterialDistributor.DataModel.Entities
{
    public class VariableCoefficientEntity
    {
        public Guid Id { get; set; }
        public int OffsetHours { get; set; }
        public int Coefficient { get; set; }
    }
}