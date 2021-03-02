using Newtonsoft.Json.Linq;
using Iis.Utility;
using Iis.Elastic.Dictionaries;
namespace Iis.Elastic.ElasticMappingProperties
{
    public class TextProperty : ElasticMappingProperty
    {
        private const string TermVectorPropName = "term_vector";
        private const string IgnoreAbovePropName = "ignore_above";
        private const int IgnoreAbove = 256;
        private TextTermVectorsEnum _termVector;
        private bool _useNestedKeyword;

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Text;
        private TextProperty() { }

        public static ElasticMappingProperty Create(string propertyName, TextTermVectorsEnum termVector, bool useNestedKeyword = false)
        {
            return CreateWithNestedProperty(
                propertyName,
                (propName) => new TextProperty{ Name = propName, _termVector = termVector, _useNestedKeyword = useNestedKeyword},
                (propName) => Create(propName, termVector, useNestedKeyword)
            );
        }

        public static ElasticMappingProperty Create(string propertyName, bool useNestedKeyword = false)
        {
            return Create(propertyName, TextTermVectorsEnum.No, useNestedKeyword);
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (_termVector != TextTermVectorsEnum.No)
            {
                result[TermVectorPropName] = _termVector.ToString().ToUnderscore();
            }

            if(_useNestedKeyword)
            {
                var field = new JObject(
                    new JProperty("keyword", new JObject(
                        new JProperty("type","keyword"),
                        new JProperty(IgnoreAbovePropName, IgnoreAbove)
                    ))
                );
                result.Add("fields", field);
            }
        }
    }
}
