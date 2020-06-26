using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.UnitTests.TestHelpers;
using IIS.Core.Materials;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests
{
    public class AssignMaterialOperatorTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public AssignMaterialOperatorTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.Materials.RemoveRange(context.Materials);
            context.Users.RemoveRange(context.Users);

            context.SaveChanges();

            _serviceProvider.Dispose();
        }
    }
}
