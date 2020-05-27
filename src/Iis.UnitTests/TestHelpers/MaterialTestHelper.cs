using System.Collections.Generic;
using System.Linq;
using Iis.Domain.Materials;
using Xunit;

namespace Iis.UnitTests.TestHelpers
{
    public static class MaterialTestHelper
    {
        internal static void AssertMaterialsOrderedByNodesCountDescending(List<Material> items)
        {
            for(var i = 1; i < items.Count(); i++)
            {
                if (items[i-1].Infos.SelectMany(p => p.Features).Select(p => p.Node).Count()
                    < items[i].Infos.SelectMany(p => p.Features).Select(p => p.Node).Count())
                {
                    Assert.True(false);
                    return;
                }
                Assert.True(true);
            }
        }
    }
}
