using System;
using System.Threading.Tasks;
using IIS.Repository.Factories;
using IIS.Repository.UnitOfWork;

namespace IIS.Repository
{
    public abstract class BaseService<TUnitOfWork> where TUnitOfWork : IUnitOfWork
    {
        private readonly IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory;

        public BaseService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
        }

        protected async Task<T> RunAsync<T>(Func<TUnitOfWork, Task<T>> action)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                var result = await action(unitOfWork);
                await unitOfWork.CommitAsync();
                return result;
            }
        }

        protected async Task<T> RunWithoutCommitAsync<T>(Func<TUnitOfWork, Task<T>> action)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                return await action(unitOfWork);
            }
        }

        protected T RunWithoutCommit<T>(Func<TUnitOfWork, T> action)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                return action(unitOfWork);
            }
        }

        protected async Task RunAsync(Func<TUnitOfWork, Task> action)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                await action(unitOfWork);
                await unitOfWork.CommitAsync();
            }
        }

        protected async Task RunAsync(Action<TUnitOfWork> action)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                action(unitOfWork);
                await unitOfWork.CommitAsync();
            }
        }

        protected void Run(Action<TUnitOfWork> action)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                action(unitOfWork);
                unitOfWork.Commit();
            }
        }

        protected async Task RunWithoutCommitAsync(Func<TUnitOfWork, Task> action)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                await action(unitOfWork);
            }
        }
    }
}
