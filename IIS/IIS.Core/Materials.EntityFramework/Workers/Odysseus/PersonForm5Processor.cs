using System;
using System.Collections.Generic;
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
            await ProcessPerson(entity, form);
            await _ontologyService.SaveNodeAsync(entity);
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

        private async Task ProcessPerson(Entity person, JObject form)
        {
            // todo: finish person processing
            var formObject = form.ToObject<Form5>();
            // question24 - family
            await Process24(person, formObject);
        }

        private void Process3(Entity person, Form5 form)
        {
            var item = form.Question3;
            if (item == null) return;

        }

        private async Task Process24(Entity person, Form5 form)
        {
            if (form.Question24 == null) return;
            var familyRelationsType = person.Type.GetProperty("familyRelations").EntityType;

            var nodes = new List<Entity>();
            foreach (var item in form.Question24)
            {
                var node = new Entity(Guid.NewGuid(), familyRelationsType);
                node.SetProperty("familyRelationKind", item.FamilyRelationKind?.Id);
                node.SetProperty("fullName", item.FullName);
                node.SetProperty("dateAndPlaceOfBirth", item.DateAndPlaceOfBirth);
                node.SetProperty("workPlaceAndPosition", item.WorkPlaceAndPosition);
                node.SetProperty("liveIn", item.LiveIn);
                nodes.Add(node);
            }
            person.SetProperty("familyRelations", nodes);
//            await Task.WhenAll(nodes.Select(n => _ontologyService.SaveNodeAsync(n)));
            foreach (var node in nodes)
                await _ontologyService.SaveNodeAsync(node);
        }


        class Form5
        {
            public class Question24Item
            {
                [JsonProperty("kinship")]
                public FormEntity FamilyRelationKind { get; set; }
                public string FullName { get; set; }
                public string DateAndPlaceOfBirth { get; set; }
                public string WorkPlaceAndPosition { get; set; }
                public string LiveIn { get; set; }
            }

            public class Question26Item
            {
                public IEnumerable<FormEntity> Phones { get; set; }
                public IEnumerable<FormEntity> Emails { get; set; }
                public IEnumerable<FormEntity> Accounts { get; set; }
            }

            public class Question28Item
            {
                public DateTime DateOfIssue { get; set; }
                public string IssuedBy { get; set; }
            }

            public class Question3Item
            {
                public DateTime BirthDate { get; set; }
                public FormEntity BirthCountry { get; set; }
                public string PostalCode { get; set; }
                public string Region { get; set; }
                public string Subregion { get; set; }
                public string City { get; set; }
            }

            public class Question25Item
            {
                [JsonExtensionData]
                public IDictionary<string, JToken> Extension { get; set; }

                public Address GetAddressWithPrefix(string prefix)
                {
                    var pairs = Extension.Where(t => t.Key.StartsWith(prefix));
                    var jo = new JObject();
                    foreach (var (key, value) in pairs)
                        jo.Add(key.Substring(prefix.Length), value);
                    return jo.ToObject<Address>();
                }

                public Address RegistrationAddress => GetAddressWithPrefix("registration");
                public Address LivingAddress => GetAddressWithPrefix("living");
            }

            public Question3Item Question3 { get; set; }
            public IEnumerable<Question24Item> Question24 { get; set; }
            public Question25Item Question25 { get; set; }
            public Question26Item Question26 { get; set; }
            public Question28Item Question28 { get; set; }
        }

        public class FormEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            [JsonProperty("__typename")]
            public string Typename { get; set; }
        }

        public class Address
        {
            public FormEntity Country { get; set; }
            public string Index { get; set; }
            public string Region { get; set; }
            public string Subregion { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public string House { get; set; }
            public string Corpus { get; set; }
            public string Flat { get; set; }
        }
    }
}
