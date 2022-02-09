using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;
using Iis.MaterialDistributor.PermanentCoefficients;
using Iis.MaterialDistributor.DataStorage;
using Iis.Interfaces.SecurityLevels;
using Iis.Security.SecurityLevels;
using Iis.MaterialDistributor.Contracts.DataStorage;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class ServicesModule
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IMaterialDistributionService, MaterialDisributionService>();
            services.AddTransient<IVariableCoefficientService, VariableCoefficientService>();
            services.AddTransient<IVariableCoefficientRuleEvaluator, VariableCoefficientRuleEvaluator>();
            services.AddTransient<IPermanentCoefficientEvaluator, PermanentCoefficientEvaluator>();
            services.AddTransient<IFinalRatingEvaluator, FinalRatingEvaluator>();

            return services;
        }
    }
}