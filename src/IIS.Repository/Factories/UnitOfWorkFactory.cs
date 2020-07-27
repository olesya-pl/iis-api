using System;
using IIS.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IIS.Repository.Factories
{
    public abstract class UnitOfWorkFactory<T> : IUnitOfWorkFactory<T> where T : IUnitOfWork
    {
        protected readonly DbContextOptions dbContextOptions;
        protected readonly IServiceProvider serviceProvider;

        public UnitOfWorkFactory(DbContextOptions dbContextOptions, IServiceProvider serviceProvider)
        {
            this.dbContextOptions = dbContextOptions;
            this.serviceProvider = serviceProvider;
        }

        public abstract T Create();
    }
}
