using System;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.Workers.Odysseus
{
    public class PersonForm5Processor : IMaterialProcessor
    {
        public const string MATERIAL_TYPE = "odysseus.person.form5";
        public const string ENTITY_TYPE = "Person";
        public const string FORM_DATA_TYPE = "form5";

        private readonly IOntologyProvider _ontologyProvider;
        private IOntologyService _ontologyService;

        public PersonForm5Processor(IOntologyProvider ontologyProvider, IOntologyService ontologyService)
        {
            _ontologyProvider = ontologyProvider;
            _ontologyService = ontologyService;
        }

        public async Task ExtractInfoAsync(Materials.Material material)
        {
            if (material.Type != MATERIAL_TYPE) return;
            var data = material.Data;
            var entity = await GetEntity(data);
            var form = GetForm(data);
            ProcessPerson(entity, form);
        }

        private string ExtractTextOfDataType(JArray data, string dataType)
        {
            var element = data.SingleOrDefault(t => t.Value<string>("Type") == dataType)
                          ?? throw new ArgumentException($"Can not find data with type '{dataType}'");
            return element.Value<string>("Text")
                   ?? throw new ArgumentException($"Can not find {dataType} text id");
        }

        private async Task<Entity> GetEntity(JArray data)
        {
            var entityIdText = ExtractTextOfDataType(data, ENTITY_TYPE);
            if (!Guid.TryParse(entityIdText, out var personId))
                throw new ArgumentException($"Can not parse {ENTITY_TYPE} id: {entityIdText}");
            var entity = await _ontologyService.LoadNodesAsync(personId, null)
                         ?? throw new ArgumentException($"{ENTITY_TYPE} with id {personId} was not found");
            var ontology = await _ontologyProvider.GetOntologyAsync();
            var type = ontology.GetEntityType(ENTITY_TYPE);
            if (!entity.Type.IsSubtypeOf(type))
                throw new ArgumentException($"Entity with id {personId} is {entity.Type.Name}, not {ENTITY_TYPE}");
            return (Entity)entity;
        }

        private JObject GetForm(JArray data)
        {
            var formText = ExtractTextOfDataType(data, FORM_DATA_TYPE);
            try
            {
                return JObject.Parse(formText);
            }
            catch (JsonException e)
            {
                throw new ArgumentException($"Expected to find inlined json in text of data {FORM_DATA_TYPE}, received json parse exception: {e.Message}", e);
            }
        }

        private void ProcessPerson(Entity person, JObject form)
        {
//            var familyRelations = person.Type.GetProperty("FamilyRelations");
//            person.GetAttributeValue("FamilyRelations");
            // todo: finish person processing
        }
    }
}
