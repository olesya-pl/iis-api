using AutoFixture;
using AutoFixture.Kernel;
using Iis.Domain.Elastic;
using Iis.Interfaces.Elastic;

namespace Iis.UnitTests.Iis.Elastic.Tests.Helpers
{
    internal static class ElasticMultiSearchParamsFixture
    {
        public static IFixture CreateFixture()
        {
            var fixture = new RecursiveAutoDataAttribute().Fixture;

            fixture.Customizations.Add(new TypeRelay(typeof(IIisElasticField), typeof(IIisElasticField)));
            fixture.Customizations.Add(new TypeRelay(typeof(IElasticMultiSearchParams), typeof(ElasticMultiSearchParams)));

            return fixture;
        }
    }
}