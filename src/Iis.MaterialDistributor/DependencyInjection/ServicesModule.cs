using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;
using Iis.MaterialDistributor.PermanentCoefficients;
using Iis.MaterialDistributor.DataStorage;
using Iis.Interfaces.SecurityLevels;
using System;
using System.Collections.Generic;
using System.Threading;
using Iis.Security.SecurityLevels;
using Iis.MaterialDistributor.Contracts.Repositories;

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
            services.AddSingleton<ISecurityLevelChecker>(provider =>
            {
                var repository = provider.GetRequiredService<IDistributionElasticRepository>();
                var task = repository.GetSecurityLevelsPlainAsync(CancellationToken.None);
                task.Wait();
                return new SecurityLevelChecker(task.Result);
            });

            return services;
        }
    }
}