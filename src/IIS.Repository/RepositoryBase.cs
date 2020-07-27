namespace IIS.Repository
{
    public abstract class RepositoryBase<TContext> : IBaseRepository<TContext>
    {
        protected TContext Context { get; private set; }

        public void SetContext(TContext context)
        {
            Context = context;
        }
    }
}
