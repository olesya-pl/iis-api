using System;
using System.Collections.Generic;
using System.Text;
using Iis.DataModel;
using IIS.Repository.Factories;
using IIS.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Iis.DbLayer.Repositories
{
    public interface IIISUnitOfWorkFactory : IUnitOfWorkFactory<IIISUnitOfWork> { }
    public class IISUnitOfWorkFactory : UnitOfWorkFactory<IIISUnitOfWork>, IIISUnitOfWorkFactory
    {
        public IISUnitOfWorkFactory(DbContextOptions dbContextOptions, IServiceProvider serviceProvider) : base(dbContextOptions, serviceProvider)
        {
        }
        public override IIISUnitOfWork Create()
        {
            return new IISUnitOfWork<OntologyContext>(dbContextOptions, serviceProvider);
        }
    }
}
