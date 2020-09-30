namespace IIS.Repository
{
    public interface IBaseRepository<in TContext>
    {
        void SetContext(TContext context);


    }
}
