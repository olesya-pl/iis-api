﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.Resolving
{
    public class RelationInfoResolver : IRelationResolver
    {
        public async Task<object> ResolveAsync(ResolveContext context)
        {
            return null;
            var relation = (Relation)context.Source;
            return relation.Attributes.Any() ? relation.Attributes : null;
        }
    }

    //public class RelationInfoAttributeResolver : IRelationResolver
    //{
    //    public async Task<object> ResolveAsync(ResolveContext context)
    //    {
    //        var attrName = context.RelationName;
    //        var relation = (Relation)context.Source;
    //        return null;
    //        //var attribute = relation.Attributes.OfType<Attribute>().SingleOrDefault(_ => _.Schema.Name == attrName);
    //        //var value = attribute?.Value;
    //        //return value;
    //    }
    //}

    public class AttributeResolver : IRelationResolver
    {
        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var attrName = context.RelationName;
            var relation = (Relation)context.Source;
            var attributeRelation = ((Entity)relation.Target).GetSingleRelation(attrName);
            var attribute = (Attribute)attributeRelation.Target;
            return attribute.Value;
        }
    }
}
