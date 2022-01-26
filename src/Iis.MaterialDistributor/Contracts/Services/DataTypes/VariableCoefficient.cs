namespace Iis.MaterialDistributor.Contracts.Services
{
    public class VariableCoefficient
    {
        public VariableCoefficient()
        {
        }

        public VariableCoefficient(int offset, int coefficient)
        {
            OffsetHours = offset;
            Coefficient = coefficient;
        }

        public int OffsetHours { get; set; }
        public int Coefficient { get; set; }
    }
}