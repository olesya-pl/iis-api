using Microsoft.EntityFrameworkCore;

namespace IIS.Repository
{
    public abstract class RepositoryBase<TContext> : IBaseRepository<TContext> where TContext: DbContext
    {
        protected TContext Context { get; private set; }

        public void SetContext(TContext context)
        {
            Context = context;
        }
    }
}
