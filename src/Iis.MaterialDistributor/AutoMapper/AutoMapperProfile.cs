using AutoMapper;
using Iis.MaterialDistributor.DataModel.Entities;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.Messages.Materials;

namespace Iis.MaterialDistributor.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Material, MaterialInfo>()
                .ConstructUsing(_ => new MaterialInfo(_));
            CreateMap<VariableCoefficientEntity, VariableCoefficient>();
            CreateMap<MaterialPermanentCoefficient, MaterialCoefficient>();
            CreateMap<UserChannelEntity, UserChannelInfo>();
            CreateMap<UserDistributionEntity, UserDistributionInfo>();
        }
    }
}