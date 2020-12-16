﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class TextProperty : ElasticMappingProperty
    {
        private const int IgnoreAbove = 256;
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Text;

        public string TermVector { get; private set; }

        public bool UseNestedKeyword { get; private set; }

        private TextProperty() { }

        public static ElasticMappingProperty Create(string dotName, string termVector, bool useNestedKeyword = false)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                {
                    Create(string.Join('.', splitted.Skip(1)), termVector, useNestedKeyword)
                });
            }

            return new TextProperty
            {
                Name = splitted[0],
                TermVector = termVector,
                UseNestedKeyword = useNestedKeyword
            };
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (!string.IsNullOrWhiteSpace(TermVector))
            {
                result["term_vector"] = TermVector;
            }

            if (UseNestedKeyword)
            {
                var field = new JObject(
                    new JProperty("keyword", new JObject(
                        new JProperty("type", "keyword"),
                        new JProperty("ignore_above", IgnoreAbove)
                    ))
                );
                result.Add("fields", field);
            }
        }
    }
}
