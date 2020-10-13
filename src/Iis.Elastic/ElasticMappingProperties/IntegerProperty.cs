﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class IntegerProperty : ElasticMappingProperty
    {
        private IntegerProperty() { }

        public static ElasticMappingProperty Create(string dotName)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                    {
                        Create(string.Join('.', splitted.Skip(1)))
                    });
            }
            return new IntegerProperty
            {
                Name = splitted[0],
            };
        }
        
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Integer;

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            
        }
    }
}
