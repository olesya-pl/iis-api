using System;

namespace IIS.Core.GraphQL.Materials
{
    public class CreateMaterialResponse
    {
        public Guid Id { get; set; }
        public bool IsDublicate { get; set; }
    }
}
