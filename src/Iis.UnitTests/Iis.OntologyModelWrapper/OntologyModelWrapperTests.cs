using Iis.DbLayer.Ontology.EntityFramework;
using Iis.Domain;
using Iis.Interfaces.Meta;
using Iis.OntologyModelWrapper;
using IIS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Iis.OntologyModelWrapper
{
    public class OntologyModelWrapperTests
    {
        [Fact]
        public void TestModelsIdentity()
        {
            var context = Utils.GetRealDbContext();
            var ontologyProvider = new OntologyProvider(context);
            var model = ontologyProvider.GetOntology();
            var schema = Utils.GetOntologySchemaFromDb();
            var wrapper = new OntologyWrapper(schema);
            CheckIdentity(model, wrapper);
        }
        private void CheckIdentity(IOntologyModel model, IOntologyModel wrapper)
        {
            CheckEntityTypes(
                model.EntityTypes.OrderBy(et => et.Name).ToList(), 
                wrapper.EntityTypes.OrderBy(et => et.Name).ToList());
        }
        private void CheckEntityTypes(List<IEntityTypeModel> modelEntityTypes, List<IEntityTypeModel> wrapperEntityTypes)
        {
            Assert.Equal(modelEntityTypes.Count, wrapperEntityTypes.Count);
            for (int i = 0; i < modelEntityTypes.Count; i++)
            {
                CheckEntityType(modelEntityTypes[i], wrapperEntityTypes[i]);
            }
        }
        private void CheckEntityType(IEntityTypeModel m, IEntityTypeModel w)
        {
            CheckNodeType(m, w);
            Assert.Equal(m.IsAbstract, w.IsAbstract);
        }
        private void CheckNodeType(INodeTypeModel m, IEntityTypeModel w)
        {
            Assert.Equal(m.Name, w.Name);
            Assert.Equal(m.ClrType, w.ClrType);
            Assert.Equal(m.Id, w.Id);
            Assert.Equal(m.CreatedAt, w.CreatedAt);
            Assert.Equal(m.Title, w.Title);
            //Assert.Equal(m.MetaSource, w.MetaSource);
            Assert.Equal(m.UpdatedAt, w.UpdatedAt);
            Assert.Equal(m.HasUniqueValues, w.HasUniqueValues);
            Assert.Equal(m.UniqueValueFieldName, w.UniqueValueFieldName);
            Assert.Equal(m.IsObjectOfStudy, w.IsObjectOfStudy);
            //Assert.Equal(m., w.);
            //Assert.Equal(m., w.);
            //Assert.Equal(m., w.);
            //Assert.Equal(m., w.);
        }
        private void CheckMeta(IMeta m, IMeta w)
        {

        }
    }
}
