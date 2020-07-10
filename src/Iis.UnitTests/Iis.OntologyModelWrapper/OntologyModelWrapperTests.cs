using Iis.DataModel;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.OntologyModelWrapper;
using Iis.OntologySchema.DataTypes;
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
            TestModelsIdentityByConnectionString("Server = localhost; Database = contour_dev_net; Username = postgres; Password = 123");
            TestModelsIdentityByConnectionString("Server = dev-db.odysseus.lcl; Database = od_dev; Username = postgres; Password = S1mpl3xTLS2709");
        }
        private void TestModelsIdentityByConnectionString(string connectionString)
        {
            var context = OntologyContext.GetContext(connectionString);
            var ontologyProvider = new OntologyProvider(context);
            var model = ontologyProvider.GetOntology();
            var schema = Utils.GetOntologySchemaFromDb(connectionString);
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
        private void CheckNodeType(INodeTypeModel m, INodeTypeModel w)
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
            CheckMeta(m.Meta, w.Meta);

            CheckEntityTypes(
                m.DirectParents.OrderBy(et => et.Id).ToList(), 
                w.DirectParents.OrderBy(et => et.Id).ToList());

            CheckEntityTypes(
                m.AllParents.OrderBy(et => et.Id).ToList(),
                w.AllParents.OrderBy(et => et.Id).ToList());

            var forbidden = new List<string>();
                //new List<string> { "EventComponent", "EventType", "Subdivision_superior", "MilitaryBase", "Subdivision" };

            if (!forbidden.Contains(m.Name))
            {
                CheckEmbeddingRelationTypes(m.Name,
                    m.AllProperties.OrderBy(et => et.Id).ToList(),
                    w.AllProperties.OrderBy(et => et.Id).ToList());
            }
        }
        private void CheckMeta(IMeta mMeta, IMeta wMeta)
        {
            var schemaMeta = (ISchemaMeta)wMeta;
            if (mMeta is EntityMeta mEntityMeta)
            {
                Assert.Equal(mEntityMeta.SortOrder, schemaMeta.SortOrder);
            }
            if (mMeta is EntityRelationMeta erMeta)
            {
                CheckInversedMeta(erMeta.Inversed, schemaMeta.Inversed);
            }
        }
        private void CheckInversedMeta(InversedRelationMeta m, IInversedRelationMeta w)
        {
            if (m == null && w == null) return;
            Assert.Equal(m.Code, w.Code);
            //Assert.False(m.Editable);
            //Assert.Null(m.SortOrder);
            Assert.Equal(m.Title, w.Title);
            Assert.Null(m.Container);
            Assert.Equal(m.Multiple, w.Multiple);
            Assert.Null(m.Validation);
        }
        private void CheckEmbeddingRelationTypes(string typeName, List<IEmbeddingRelationTypeModel> m, List<IEmbeddingRelationTypeModel> w)
        {
            var l1 = m.Where(r => !w.Any(t => t.Name == r.Name)).ToList();
            var l2 = w.Where(r => !m.Any(t => t.Name == r.Name)).ToList();
            //Assert.Equal(m.Count, w.Count);
            for (int i = 0; i < m.Count; i++)
            {
                var wrapper = w.SingleOrDefault(emb => emb.Id == m[i].Id);
                if (wrapper != null)
                {
                    CheckEmbeddingRelationType(m[i], wrapper);
                }
            }
        }
        private void CheckEmbeddingRelationType(IEmbeddingRelationTypeModel m, IEmbeddingRelationTypeModel w)
        {
            CheckNodeType(m, w);
        }
    }
}
