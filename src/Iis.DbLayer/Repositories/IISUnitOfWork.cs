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
        IOntologyRepository OntologyRepository { get; }
    }
    public class IISUnitOfWork<TContext> : UnitOfWork<TContext>, IIISUnitOfWork
        where TContext : DbContext
    {
        public IISUnitOfWork(IGenericFactory factory, DbContextOptions dbContextOptions, IServiceProvider serviceProvider) : base(factory, dbContextOptions, serviceProvider)
        {
        }

        public IMaterialRepository MaterialRepository => ResolveRepository<IMaterialRepository>();

        public IOntologyRepository OntologyRepository => ResolveRepository<IOntologyRepository>();
        //public IDataRepository DataRepository => ResolveRepository<IDataRepository>();
        //public IDataFilesRepository DataFilesRepository => ResolveRepository<IDataFilesRepository>();
        //public IInvestorsRepository InvestorsRepository => ResolveRepository<IInvestorsRepository>();
        //public ISettingsRepository SettingsRepository => ResolveRepository<ISettingsRepository>();
        //public ICompaniesRepository CompaniesRepository => ResolveRepository<ICompaniesRepository>();

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
