using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Iis.Roles;
using Iis.DataModel;
using Iis.DataModel.Materials;

namespace Iis.UnitTests.Materials
{
    public class MaterialUpdateTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public MaterialUpdateTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            
            context.MaterialSigns.RemoveRange(context.MaterialSigns);
            context.MaterialSignTypes.RemoveRange(context.MaterialSignTypes);

            context.SaveChanges();

            _serviceProvider.Dispose();
        }
        [Theory(DisplayName = "Get ProcessedStatus list"), RecursiveAutoData]
        public async Task GetProcessedStatuses(MaterialSignTypeEntity typeEntity,
            MaterialSignEntity processed,
            MaterialSignEntity notProcessed
        )
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            typeEntity.Name = "ProcessedStatus";
            typeEntity.Title = "Обробка";
            typeEntity.MaterialSigns = null;

            processed.MaterialSignType = null;
            processed.MaterialSignTypeId = typeEntity.Id;
            processed.Title = "processed";

            notProcessed.MaterialSignType = null;
            notProcessed.MaterialSignTypeId = typeEntity.Id;
            notProcessed.Title = "not" 

            context.MaterialSignTypes.Add(typeEntity);
            context.MaterialSigns.Add(processed);
            context.MaterialSigns.Add(notProcessed);

            context.SaveChanges();
            

        }
        // [Theory(DisplayName = "")]
        // public async Task UodateProcessStatus()
        // {

        // }
    }
}