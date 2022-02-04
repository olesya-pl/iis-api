using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;
using Iis.MaterialDistributor.PermanentCoefficients;
using Iis.MaterialDistributor.DataStorage;
using Iis.Interfaces.SecurityLevels;
using System;
using System.Collections.Generic;
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
            services.AddTransient<IFinalRatingEvaluator, FinalRatingEvaluator>();
            services.AddSingleton<IDistributionData, DistributionData>();
            services.AddSingleton<ISecurityLevelChecker>(_ => new SecurityLevelChecker(new List<SecurityLevelPlain>
                {
                    new SecurityLevelPlain { Id = Guid.NewGuid(), Name = "Root", UniqueIndex = 1 }
                }));

            return services;
        }
    }
}