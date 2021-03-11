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

        public IOntologySchema CreateBaseTestOntology()
        {
            var objectType = _schema.CreateEntityType(EntityTypeNames.Object.ToString(), "Base Object", true);
            _schema.CreateAttributeType(objectType.Id, "title", "Title");
            _schema.CreateAttributeType(objectType.Id, "__title", "Description", meta: new SchemaMeta { Formula = "{title};\"No Name Object\"" });

            var enumType = _schema.CreateEntityType(EntityTypeNames.Enum.ToString(), "Base Enum", true);
            _schema.CreateAttributeType(enumType.Id, "name", "Name", ScalarType.String, EmbeddingOptions.Required);
            _schema.CreateAttributeType(enumType.Id, "sortOrder", "Sort Order", ScalarType.Int);

            var affiliationType = _schema.CreateEntityType(EntityTypeNames.ObjectAffiliation.ToString(), "Affiliation", false, enumType.Id);
            var importanceType = _schema.CreateEntityType(EntityTypeNames.ObjectImportance.ToString(), "Importance", false, enumType.Id);

            var signType = _schema.CreateEntityType(EntityTypeNames.ObjectSign.ToString(), "Base Sign", true);
            _schema.CreateAttributeType(signType.Id, "value", "Value");

            _schema.CreateEntityType(EntityTypeNames.EmailSign.ToString(), "Email Sign", false, enumType.Id);

            var objectOfStudyType = _schema.CreateEntityType(EntityTypeNames.ObjectOfStudy.ToString(), "Object Of Study", true, objectType.Id);
            _schema.CreateRelationType(objectOfStudyType.Id, affiliationType.Id, "affiliation", "Affiliation");
            _schema.CreateRelationType(objectOfStudyType.Id, importanceType.Id, "importance", "Importance");
            _schema.CreateRelationType(objectOfStudyType.Id, signType.Id, "sign", "Sign");
            _schema.CreateAttributeType(objectOfStudyType.Id, "lastConfirmedAt", "Last Confirmed At", ScalarType.Date);

            var wikiType = _schema.CreateEntityType(EntityTypeNames.Wiki.ToString(), "Base Wiki", true, objectType.Id);
            _schema.CreateAttributeType(wikiType.Id, "photo", "Photo", ScalarType.File, EmbeddingOptions.Multiple);

            return _schema;
        }
    }
}
