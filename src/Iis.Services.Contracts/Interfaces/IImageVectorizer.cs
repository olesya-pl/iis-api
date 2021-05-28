using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IImageVectorizer
    {
        Task<IReadOnlyCollection<decimal[]>> VectorizeImage(byte[] fileContent, string fileName);
    }
}
