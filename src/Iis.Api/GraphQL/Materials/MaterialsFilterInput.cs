﻿using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;
using Iis.Api.GraphQL.Common;
using IIS.Core.GraphQL.ChangeHistory;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialsFilterInput
    {
        public string Suggestion { get; set; }
        public string RelationsState { get; set; }
        [GraphQLType(typeof(ListType<NonNullType<StringType>>))]
        public List<string> CherryPickedItems { get; set; } = new List<string>();
        public List<Property> FilteredItems { get; set; } = new List<Property>();
        public NullableDateRangeFilter UploadDateRange { get; set; }
    }
}
