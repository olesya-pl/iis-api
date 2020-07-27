using IIS.Repository.UnitOfWork;

namespace IIS.Repository.Factories
{
    public interface IUnitOfWorkFactory<T> where T : IUnitOfWork
    {
        T Create();
    }
}
