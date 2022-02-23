using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;
using Iis.Interfaces.SecurityLevels;
using Iis.Security.SecurityLevels;

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
            services.AddSingleton<IChannelCoefficientEvaluator, ChannelCoefficientEvaluator>();
            services.AddSingleton<ISecurityLevelChecker, SecurityLevelChecker>();
            services.AddTransient<IFinalRatingEvaluator, FinalRatingEvaluator>();

            return services;
        }
    }
}