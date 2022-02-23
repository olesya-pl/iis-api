using Iis.Services.Contracts.Ontology;
using System;

namespace Iis.Api.Controllers.Dto
{
    public class GetEntityParam
    {
        public Guid Id { get; set; }
        public GetEntityOptions Options { get; set; }
    }
}
