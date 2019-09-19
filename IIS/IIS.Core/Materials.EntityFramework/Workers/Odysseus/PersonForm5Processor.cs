using System.Threading.Tasks;

namespace IIS.Core.Materials.EntityFramework.Workers.Odysseus
{
    public class PersonForm5Processor : IMaterialProcessor
    {
        public Task ExtractInfoAsync(Materials.Material material)
        {
//            throw new System.NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
