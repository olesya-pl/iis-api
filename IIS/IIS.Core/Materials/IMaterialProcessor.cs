using System.Threading.Tasks;

namespace IIS.Core.Materials
{
    public interface IMaterialProcessor
    {
        Task ExtractInfoAsync(Material material);
    }
}
