using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace Iis.UnitTests.MaterialDistributor
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() =>
            {
                return new Fixture().Customize(new AutoMoqCustomization());
            })
        {
        }
    }
}