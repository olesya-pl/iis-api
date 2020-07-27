using System;
using System.Threading.Tasks;

namespace IIS.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task CommitAsync();

        void Commit();
    }
}
