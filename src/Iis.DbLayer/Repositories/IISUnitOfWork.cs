using System;
using System.Collections.Generic;
using System.Text;
using Iis.DataModel;
using Iis.DbLayer.Ontology.EntityFramework;
using IIS.Repository;
using IIS.Repository.Factories;
using IIS.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
