using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.ChangeParameters;
using Iis.OntologySchema.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.UnitTests.Iis.OntologySchema
{
    public enum EntityTypeNames : byte
    {
        ObjectOfStudy,
        FuzzyDate,
        ObjectSign,
        Event,
        Enum,
        Wiki,
        Object,
        ObjectAffiliation,
        ObjectImportance,
        EmailSign,
    }

    public class OntologyDataCreator
    {
        IOntologySchema _schema;

        public OntologyDataCreator(IOntologySchema schema)
        {
            _schema = schema;
        }

        public static (IOntologySchema schema, OntologyDataCreator creator) GetBaseTestOntology()
        {
            var schema = Utils.GetEmptyOntologySchema();
            var creator = new OntologyDataCreator(schema);
            creator.CreateBaseTestOntology();
            return (schema, creator);
        }

        public INodeTypeLinked CreateEntityType(string name, string title = null, bool isAbstract = false, Guid? ancestorId = null)
        {
            var updateParameter = new NodeTypeUpdateParameter
            {
                Name = name,
                Title = title ?? name,
                IsAbstract = isAbstract
            };
            var nodeType = _schema.UpdateNodeType(updateParameter);
            
            if (ancestorId != null)
            {
                _schema.SetInheritance(nodeType.Id, (Guid)ancestorId);
            }
            return nodeType;
        }
        public INodeTypeLinked CreateAttributeType(
            Guid parentId, 
            string name,
            string title = null,
            ScalarType scalarType = ScalarType.String,
            EmbeddingOptions embeddingOptions = EmbeddingOptions.Optional,
            ISchemaMeta meta = null)
        {
            var updateParameter = new NodeTypeUpdateParameter
            {
                Name = name,
                Title = title ?? name,
                EmbeddingOptions = embeddingOptions,
                ScalarType = scalarType,
                ParentTypeId = parentId,
                Meta = meta == null ? null : JsonConvert.SerializeObject(meta)
            };
            return _schema.UpdateNodeType(updateParameter);
        }
        public INodeTypeLinked CreateRelationType(
            Guid sourceId,
            Guid targetId,
            string name,
            string title = null,
            EmbeddingOptions embeddingOptions = EmbeddingOptions.Optional,
            ISchemaMeta meta = null)
        {
            var updateParameter = new NodeTypeUpdateParameter
            {
                Name = name,
                Title = title ?? name,
                EmbeddingOptions = embeddingOptions,
                ParentTypeId = sourceId,
                TargetTypeId = targetId,
                Meta = meta == null ? null : JsonConvert.SerializeObject(meta)
            };
            return _schema.UpdateNodeType(updateParameter);
        }

        public IOntologySchema CreateBaseTestOntology()
        {
            var objectType = CreateEntityType(EntityTypeNames.Object.ToString(), "Base Object", true);
            CreateAttributeType(objectType.Id, "title", "Title");
            CreateAttributeType(objectType.Id, "__title", "Description", meta: new SchemaMeta { Formula = "{title};\"No Name Object\"" });

            var enumType = CreateEntityType(EntityTypeNames.Enum.ToString(), "Base Enum", true);
            CreateAttributeType(enumType.Id, "name", "Name", ScalarType.String, EmbeddingOptions.Required);
            CreateAttributeType(enumType.Id, "sortOrder", "Sort Order", ScalarType.Int);

            var affiliationType = CreateEntityType(EntityTypeNames.ObjectAffiliation.ToString(), "Affiliation", false, enumType.Id);
            var importanceType = CreateEntityType(EntityTypeNames.ObjectImportance.ToString(), "Importance", false, enumType.Id);

            var signType = CreateEntityType(EntityTypeNames.ObjectSign.ToString(), "Base Sign", true);
            CreateAttributeType(signType.Id, "value", "Value");

            CreateEntityType(EntityTypeNames.EmailSign.ToString(), "Email Sign", false, enumType.Id);

            var objectOfStudyType = CreateEntityType(EntityTypeNames.ObjectOfStudy.ToString(), "Object Of Study", true, objectType.Id);
            CreateRelationType(objectOfStudyType.Id, affiliationType.Id, "affiliation", "Affiliation");
            CreateRelationType(objectOfStudyType.Id, importanceType.Id, "importance", "Importance");
            CreateRelationType(objectOfStudyType.Id, signType.Id, "sign", "Sign");
            CreateAttributeType(objectOfStudyType.Id, "lastConfirmedAt", "Last Confirmed At", ScalarType.Date);

            var wikiType = CreateEntityType(EntityTypeNames.Wiki.ToString(), "Base Wiki", true, objectType.Id);
            CreateAttributeType(wikiType.Id, "photo", "Photo", ScalarType.File, EmbeddingOptions.Multiple);

            return _schema;
        }
    }
}
