using System;
using IIS.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IIS.Repository.Factories
{
    public abstract class UnitOfWorkFactory<T> : IUnitOfWorkFactory<T> where T : IUnitOfWork
    {
        protected readonly IGenericFactory factory;
        protected readonly DbContextOptions dbContextOptions;
        protected readonly IServiceProvider serviceProvider;

        public UnitOfWorkFactory(IGenericFactory factory, DbContextOptions dbContextOptions, IServiceProvider serviceProvider)
        {
            this.factory = factory;
            this.dbContextOptions = dbContextOptions;
            this.serviceProvider = serviceProvider;
        }

        public abstract T Create();
    }
}
