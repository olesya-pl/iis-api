using HotChocolate;
using HotChocolate.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResNode
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public ResNodeExtra Extra { get; set; }
    }
}
