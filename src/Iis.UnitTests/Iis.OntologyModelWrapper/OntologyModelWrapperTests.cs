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

            var subdivision = schema.GetEntityTypeByName("Subdivision");
            if (subdivision != null)
            {
                var dp = subdivision.GetDirectProperties().ToList();
                var ap = subdivision.GetAllProperties().ToList();
            }
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
            //Assert.Equal(m.Id, w.Id);
            Assert.Equal(m.CreatedAt, w.CreatedAt);
            Assert.Equal(m.Title, w.Title);
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
                CheckEntityMeta(mEntityMeta, schemaMeta);
            }
            if (mMeta is RelationMetaBase)
            {
                CheckRelationMetaBase((RelationMetaBase)mMeta, schemaMeta);
            }
            if (mMeta is EntityRelationMeta erMeta)
            {
                CheckEntityRelationMeta(erMeta, schemaMeta);
            }
            if (mMeta is AttributeRelationMeta arMeta)
            {
                Assert.Equal(arMeta.Formula, schemaMeta.Formula);
                Assert.Equal(arMeta.Format, schemaMeta.Format);
            }
            if (mMeta is AttributeMeta aMeta)
            {
                Assert.Null(aMeta.Kind);
                Assert.Null(aMeta.Validation);
            }
        }
        private void CheckEntityMeta(EntityMeta m, ISchemaMeta w)
        {
            Assert.Equal(m.SortOrder, w.SortOrder);
            Assert.Equal(m.ExposeOnApi, w.ExposeOnApi);
            Assert.Equal(m.HasFewEntities, w.HasFewEntities);
            Assert.Equal(m.AcceptsEmbeddedOperations, w.AcceptsEmbeddedOperations);
            CheckContainers(m.Container, w.Container);
            CheckFormFields(m.FormField, w.FormField);
        }
        private void CheckEntityRelationMeta(EntityRelationMeta m, ISchemaMeta w)
        {
            Assert.Equal(m.Type, w.Type);
            Assert.Equal(m.AcceptsEntityOperations, w.AcceptsEntityOperations);
            Assert.Equal(m.TargetTypes, w.TargetTypes);
            CheckInversedMeta(m.Inversed, w.Inversed);
        }
        private void CheckRelationMetaBase(RelationMetaBase m, ISchemaMeta w)
        {
            Assert.Equal(m.Multiple, w.Multiple);
            Assert.Equal(m.SortOrder, w.SortOrder);
            Assert.Equal(m.Title, w.Title);
            Assert.Equal(m.Validation?.Required, w.Validation?.Required);
            CheckFormFields(m.FormField, w.FormField);
            CheckContainers(m.Container, w.Container);
        }
        private void CheckContainers(IContainerMeta m, IContainerMeta w)
        {
            if (m == null && w == null) return;
            Assert.Equal(m.Id, w.Id);
            Assert.Equal(m.Title, w.Title);
            Assert.Equal(m.Type, w.Type);
        }
        private void CheckFormFields(IFormField m, IFormField w)
        {
            if (m == null && w == null) return;
            Assert.Equal(m.Type, w.Type);
            Assert.Equal(m.HasIndexColumn, w.HasIndexColumn);
            Assert.Equal(m.Lines, w.Lines);
            Assert.Equal(m.Hint, w.Hint);
            Assert.Equal(m.Icon, w.Icon);
            Assert.Equal(m.IncludeParent, w.IncludeParent);
            Assert.Equal(m.Layout, w.Layout);
            Assert.Equal(m.RadioType, w.RadioType);
        }
        private void CheckInversedMeta(IInversedRelationMeta m, IInversedRelationMeta w)
        {
            if (m == null && w == null) return;
            Assert.Equal(m.Code, w.Code);
            Assert.Equal(m.Title, w.Title);
            Assert.Equal(m.Multiple, w.Multiple);
           
            Assert.False(m.Editable);
            Assert.Null(m.FormField);
            Assert.Null(m.SortOrder);
            Assert.Null(m.Container);
            Assert.Null(m.Validation);
        }
        private void CheckEmbeddingRelationTypes(string typeName, List<IEmbeddingRelationTypeModel> m, List<IEmbeddingRelationTypeModel> w)
        {
            var l1 = m.Where(r => !w.Any(t => t.Name == r.Name)).ToList();
            var l2 = w.Where(r => !m.Any(t => t.Name == r.Name)).ToList();
            
            Assert.Equal(m.Count, w.Count);
            for (int i = 0; i < m.Count; i++)
            {
                var wrapper = w.SingleOrDefault(emb => emb.Id == m[i].Id && emb.IsInversed == m[i].IsInversed);
                if (wrapper == null && m[i].IsInversed)
                {
                    wrapper = w.SingleOrDefault(emb => emb.Name == m[i].Name && emb.IsInversed);
                }
                CheckEmbeddingRelationType(m[i], wrapper);
            }
        }
        private void CheckEmbeddingRelationType(IEmbeddingRelationTypeModel m, IEmbeddingRelationTypeModel w)
        {
            CheckNodeType(m, w);
            Assert.Equal(m.DirectRelationType?.Id, w.DirectRelationType?.Id);
            CheckMeta(m.EmbeddingMeta, w.EmbeddingMeta);
            Assert.Equal(m.EmbeddingOptions, w.EmbeddingOptions);
            Assert.Equal(m.EntityType?.Id, w.EntityType?.Id);
            Assert.Equal(m.AttributeType?.Id, w.AttributeType?.Id);
            Assert.Equal(m.AttributeType?.ScalarTypeEnum, w.AttributeType?.ScalarTypeEnum);
            Assert.Equal(m.IsAttributeType, w.IsAttributeType);
            Assert.Equal(m.IsEntityType, w.IsEntityType);
            Assert.Equal(m.IsInversed, w.IsInversed);
            //Assert.Equal(
            //    m.RelationTypes.Select(rt => rt.Id).OrderBy(id => id),
            //    m.RelationTypes.Select(rt => rt.Id).OrderBy(id => id));
            Assert.Equal(m.TargetType?.Id, w.TargetType?.Id);
            Assert.Equal(m.AcceptsOperation(EntityOperation.Create), w.AcceptsOperation(EntityOperation.Create));
            Assert.Equal(m.AcceptsOperation(EntityOperation.Delete), w.AcceptsOperation(EntityOperation.Delete));
            Assert.Equal(m.AcceptsOperation(EntityOperation.Update), w.AcceptsOperation(EntityOperation.Update));
        }
    }
}
