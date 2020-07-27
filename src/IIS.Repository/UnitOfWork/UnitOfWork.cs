using System;
using System.Threading.Tasks;
using IIS.Repository.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IIS.Repository.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork
        where TContext : DbContext
    {
        protected readonly TContext context;
        protected readonly IServiceProvider serviceProvider;

        public UnitOfWork(DbContextOptions dbContextOptions, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            context = serviceProvider.GetService<TContext>();
        }

        public virtual async Task CommitAsync()
        {
            await context.SaveChangesAsync().ConfigureAwait(true);
        }

        public virtual void Commit()
        {
            context.SaveChanges();
        }

        public virtual void Dispose()
        {
            context.Dispose();
        }

        protected virtual T ResolveRepository<T>()
        {
            var key = typeof(T);
            var repository = (T)serviceProvider.GetService(key);
            if (repository == null)
            {
                throw new ApplicationException($"Requested repository with the type '{key.Name}' is not registered.");
            }

            if (repository is IBaseRepository<DbContext> baseRepository)
            {
                baseRepository.SetContext(context);
            }

            return repository;
        }
    }
}
