using AutoMapper;
using Iis.MaterialDistributor.DataModel.Entities;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.Messages.Materials;
using Iis.MaterialDistributor.Contracts.Services.DataTypes;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialDistributor.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<VariableCoefficientEntity, VariableCoefficient>();
            CreateMap<Material, MaterialInfo>()
                .ConstructUsing(_ => new MaterialInfo(_));

            CreateMap<MaterialPermanentCoefficient, MaterialCoefficient>();
        }
    }
}