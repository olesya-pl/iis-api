using System;
using System.Collections.Generic;
using System.Text;
using Iis.DataModel;
using IIS.Repository;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public interface IOntologyRepository { }
    public class OntologyRepository : RepositoryBase<OntologyContext>, IOntologyRepository
    {

    }
}
