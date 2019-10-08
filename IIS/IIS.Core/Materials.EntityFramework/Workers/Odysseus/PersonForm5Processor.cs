using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework.Context;
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
        private readonly IOntologyService _ontologyService;
        private readonly OntologyContext _ontologyContext;

        public PersonForm5Processor(IOntologyProvider ontologyProvider, IOntologyService ontologyService, OntologyContext ontologyContext)
        {
            _ontologyProvider = ontologyProvider;
            _ontologyService = ontologyService;
            _ontologyContext = ontologyContext;
        }

        public async Task ExtractInfoAsync(Materials.Material material)
        {
            if (material.Type != MATERIAL_TYPE) return;
            var data = material.Data;
            var entity = await GetEntity(data, material.Id);
            var form = GetForm(data);
            await ProcessPerson(entity, form);
            await _ontologyContext.SaveChangesAsync();
            await _ontologyService.SaveNodeAsync(entity);
        }

        private string ExtractTextOfDataType(JArray data, string dataType)
        {
            var element = data.SingleOrDefault(t => t.Value<string>("Type") == dataType)
                          ?? throw new ArgumentException($"Can not find data with type '{dataType}'");
            return element.Value<string>("Text")
                   ?? throw new ArgumentException($"Can not find {dataType} text id");
        }

        private JObject ExtractObjectOfDataType(JArray data, string dataType)
        {
            var element = data.SingleOrDefault(t => t.Value<string>("Type") == dataType)
                          ?? throw new ArgumentException($"Can not find data with type '{dataType}'");
            return element as JObject;
        }

        private async Task<Entity> GetEntity(JArray data, Guid materialId)
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

            // save mapping
            var info = new MaterialInfo
            {
                Id = Guid.NewGuid(),
                Data = ExtractObjectOfDataType(data, ENTITY_TYPE).ToString(),
                MaterialId = materialId,
                Source = GetType().FullName,
                SourceType = nameof(PersonForm5Processor),
                SourceVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion,
            };
            var feature = new MaterialFeature
            {
                Id = Guid.NewGuid(),
                MaterialInfoId = info.Id,
                NodeId = entity.Id,
                Relation = ENTITY_TYPE,
                Value = entityIdText,
            };
            _ontologyContext.Add(info);
            _ontologyContext.Add(feature);

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
            var formObject = form.ToObject<Form5>();
            await Process3(person, formObject);
            await Process24(person, formObject);
            await Process25(person, formObject);
            await Process26(person, formObject);
            await Process28(person, formObject);
        }

        private async Task Process3(Entity person, Form5 form)
        {
            var item = form.Question3;
            if (item == null) throw new ArgumentException("Question 3 was not found");
            person.SetProperty("birthDate", item.BirthDate);
            var address = person.GetRelationType("birthPlace").EntityType;
            var node = new Entity(Guid.NewGuid(), address);
            node.SetProperty("zipCode", item.ZipCode);
            node.SetProperty("region", item.Region);
            node.SetProperty("subregion", item.Subregion);
            node.SetProperty("city", item.City);
            person.SetProperty("birthPlace", node);
            await _ontologyService.SaveNodeAsync(node);
        }

        private async Task Process24(Entity person, Form5 form)
        {
            if (form.Question24 == null) throw new ArgumentException("Question 24 was not found");
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
            foreach (var node in nodes)
                await _ontologyService.SaveNodeAsync(node);
        }

        private async Task Process25(Entity person, Form5 form)
        {
            async Task assignAddress(string propertyName, Address address)
            {
                if (address == null) return;
                var type = person.GetRelationType(propertyName).EntityType;
                var node = new Entity(Guid.NewGuid(), type);
                node.SetProperty("zipCode", address.ZipCode);
                node.SetProperty("region", address.Region);
                node.SetProperty("city", address.City);
                node.SetProperty("subregion", address.Subregion);
                node.SetProperty("street", address.Street);
                node.SetProperty("building", address.House);
                node.SetProperty("apartment", address.Apartment);
                person.SetProperty(propertyName, node);
                await _ontologyService.SaveNodeAsync(node);
            }

            var item = form.Question25;
            if (item == null) throw new ArgumentException("Question 25 was not found");
            await assignAddress("registrationPlace", item.RegistrationPlace);
            await assignAddress("livingPlace", item.LivingPlace);
        }

        private static string StripPrefix(string text, string prefix)
        {
            return text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
        }

        private async Task Process26(Entity person, Form5 form)
        {
            var ontology = await _ontologyProvider.GetOntologyAsync();
            async Task assignSigns(string propertyName, IEnumerable<SignEntity> signs)
            {
                if (signs == null) return;
                var type = person.GetRelationType(propertyName).EntityType;
                var nodes = new List<Entity>();
                foreach (var sign in signs)
                {
                    var targetTypeName = StripPrefix(sign.Typename, "Entity");
                    EntityType targetType;
                    if (targetTypeName == type.Name)
                    {
                        targetType = type;
                    }
                    else
                    {
                        targetType = ontology.GetEntityType(targetTypeName)
                                     ?? throw new ArgumentException($"Type {targetTypeName} was not found");
                        if (!targetType.IsSubtypeOf(type))
                            throw new ArgumentException($"{targetTypeName} is not subtype of {type.Name}");
                    }
                    var node = new Entity(Guid.NewGuid(), targetType);
                    node.SetProperty("value", sign.Value);
                    nodes.Add(node);
                }

                foreach (var node in nodes)
                    await _ontologyService.SaveNodeAsync(node);

                person.SetProperty(propertyName, nodes);
            }

            var item = form.Question26;
            if (item == null) throw new ArgumentException("Question 26 was not found");
            await assignSigns("phoneSign", item.Phones);
            await assignSigns("emailSign", item.EmailSign);
            await assignSigns("socialNetworkSign", item.SocialNetworkSign);
        }

        private async Task Process28(Entity person, Form5 form)
        {
            var item = form.Question28;
            if (item == null) throw new ArgumentException("Question 28 was not found");

            var passport = (Entity) person.GetProperty("passport");
            if (passport == null)
            {
                passport = new Entity(Guid.NewGuid(), person.GetRelationType("passport").EntityType);
                person.SetProperty("passport", passport);
            }
            passport.SetProperty("issueInfo", item.IssuedBy);
            passport.SetProperty("issueDate", item.DateOfIssue);
            await _ontologyService.SaveNodeAsync(passport);
        }


        class Form5
        {
            public class Question24Item
            {
                public FormEntity FamilyRelationKind { get; set; }
                public string FullName { get; set; }
                public string DateAndPlaceOfBirth { get; set; }
                public string WorkPlaceAndPosition { get; set; }
                public string LiveIn { get; set; }
            }

            public class Question26Item
            {
                public IEnumerable<SignEntity> Phones { get; set; }
                public IEnumerable<SignEntity> EmailSign { get; set; }
                public IEnumerable<SignEntity> SocialNetworkSign { get; set; }
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
                public string ZipCode { get; set; }
                public string Region { get; set; }
                public string Subregion { get; set; }
                public string City { get; set; }
            }

            public class Question25Item
            {
                [JsonExtensionData]
                public IDictionary<string, JToken> Extension { get; set; }

                public Address RegistrationPlace { get; set; }
                public Address LivingPlace { get; set; }
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

        public class SignEntity
        {
            public string Value { get; set; }
            [JsonProperty("__typename")]
            public string Typename { get; set; }
        }

        public class Address
        {
            public FormEntity Country { get; set; }
            public string ZipCode { get; set; }
            public string Region { get; set; }
            public string Subregion { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public string House { get; set; }
            public string Corpus { get; set; }
            public string Apartment { get; set; }
        }
    }
}
