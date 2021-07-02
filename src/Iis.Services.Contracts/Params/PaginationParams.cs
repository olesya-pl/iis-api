using System;
namespace Iis.Services.Contracts.Params
{
    public class PaginationParams
    {
        public int Page { get; }
        public int Size { get; }
        public PaginationParams(int page, int size)
        {
            if(page <= 0) throw new ArgumentOutOfRangeException(nameof(page), page, "Parameter should be above zero.");
            if(size <= 0) throw new ArgumentOutOfRangeException(nameof(size), size, "Parameter should be above zero.");

            Page = page;
            Size = size;
        }

        public (int From, int Size) ToElasticPage()
        {
            return (Page * Size - Size, Size);
        }

        public (int Skip, int Take) ToEFPage() => (Page * Size - Size, Size);
    }
}