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
        NicknameSign
    }

    public class OntologyDataCreator
    {
        IOntologySchema _schema;

        public OntologyDataCreator(IOntologySchema schema)
        {
            _schema = schema;
        }

        public INodeTypeLinked CreateEntity(string name, string title = null, bool isAbstract = false, Guid? ancestorId = null)
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
        public INodeTypeLinked CreateAttribute(
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
        public INodeTypeLinked CreateRelation(
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

        public IOntologySchema CreateTestOntology1()
        {
            var objectType = CreateEntity(EntityTypeNames.Object.ToString(), "Base Object", true);
            CreateAttribute(objectType.Id, "title", "Title");
            CreateAttribute(objectType.Id, "__title", "Description", meta: new SchemaMeta { Formula = "{title};\"No Name Object\"" });

            var enumType = CreateEntity(EntityTypeNames.Enum.ToString(), "Base Enum", true);
            CreateAttribute(enumType.Id, "name", "Name");
            CreateAttribute(enumType.Id, "value", "Value");

            var affiliationType = CreateEntity(EntityTypeNames.ObjectAffiliation.ToString(), "Affiliation", false, enumType.Id);
            var importanceType = CreateEntity(EntityTypeNames.ObjectImportance.ToString(), "Importance", false, enumType.Id);

            var signType = CreateEntity(EntityTypeNames.ObjectSign.ToString(), "Base Sign", true);
            CreateAttribute(signType.Id, "value", "Value");

            CreateEntity(EntityTypeNames.EmailSign.ToString(), "Email Sign", false, enumType.Id);

            var objectOfStudyType = CreateEntity(EntityTypeNames.ObjectOfStudy.ToString(), "Object Of Study", true, objectType.Id);
            CreateRelation(objectOfStudyType.Id, affiliationType.Id, "affiliation", "Affiliation");
            CreateRelation(objectOfStudyType.Id, importanceType.Id, "importance", "Importance");
            CreateRelation(objectOfStudyType.Id, signType.Id, "sign", "Sign");
            CreateAttribute(objectOfStudyType.Id, "lastConfirmedAt", "Last Confirmed At", ScalarType.Date);

            var wikiType = CreateEntity(EntityTypeNames.Wiki.ToString(), "Base Wiki", true, objectType.Id);
            CreateAttribute(wikiType.Id, "photo", "Photo", ScalarType.File, EmbeddingOptions.Multiple);

            return _schema;
        }
    }
}
