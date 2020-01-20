using System.Threading.Tasks;
using Iis.Domain.Materials;

namespace IIS.Core.Materials
{
    public interface IMaterialProcessor
    {
        Task ExtractInfoAsync(Material material);
    }
}
