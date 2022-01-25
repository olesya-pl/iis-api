using System;

namespace Iis.Messages.Materials
{
    public class MaterialCoefficientEvaluatedEventMessage
    {
        public MaterialCoefficient[] MaterialCoefficients { get; set; }
    }

    public class MaterialCoefficient
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
    }
}