using System;
using IIS.Repository;
using IIS.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    public interface IIISUnitOfWork : IUnitOfWork
    {
        IMaterialRepository MaterialRepository { get; }
        IAnnotationsRepository AnnotationsRepository {get; }
        IFlightRadarRepository FlightRadarRepository { get; }
        IReportRepository ReportRepository { get; }
        IElasticFieldsRepository ElasticFieldsRepository { get; }
        IFileRepository FileRepository { get; }
        IAliasRepository AliasRepository { get; }
        IThemeRepository ThemeRepository { get; }
        IChangeHistoryRepository ChangeHistoryRepository { get; }
        IUserRepository UserRepository { get; }
        ITowerLocationRepository TowerLocationRepository { get; }
        
        IMaterialSignRepository MaterialSignRepository { get; }
    }
    public class IISUnitOfWork<TContext> : UnitOfWork<TContext>, IIISUnitOfWork
        where TContext : DbContext
    {
        public IISUnitOfWork(DbContextOptions dbContextOptions, IServiceProvider serviceProvider) : base(dbContextOptions, serviceProvider)
        {
        }

        public IMaterialRepository MaterialRepository => ResolveRepository<IMaterialRepository>();

        public IAnnotationsRepository AnnotationsRepository => ResolveRepository<IAnnotationsRepository>();

        public IFlightRadarRepository FlightRadarRepository => ResolveRepository<IFlightRadarRepository>();

        public IReportRepository ReportRepository => ResolveRepository<IReportRepository>();
        
        public IElasticFieldsRepository ElasticFieldsRepository => ResolveRepository<IElasticFieldsRepository>();

        public IFileRepository FileRepository => ResolveRepository<IFileRepository>();
        
        public IAliasRepository AliasRepository => ResolveRepository<IAliasRepository>();
        
        public IThemeRepository ThemeRepository => ResolveRepository<IThemeRepository>();

        public IChangeHistoryRepository ChangeHistoryRepository => ResolveRepository<IChangeHistoryRepository>();

        public IUserRepository UserRepository => ResolveRepository<IUserRepository>();
        
        public ITowerLocationRepository TowerLocationRepository => ResolveRepository<ITowerLocationRepository>();
        public IMaterialSignRepository MaterialSignRepository => ResolveRepository<IMaterialSignRepository>();

        protected override T ResolveRepository<T>()
        {
            var repository = base.ResolveRepository<T>();

            if (repository is IBaseRepository<TContext> baseRepository)
            {
                baseRepository.SetContext(context);
            }

            return repository;
        }
    }
}
