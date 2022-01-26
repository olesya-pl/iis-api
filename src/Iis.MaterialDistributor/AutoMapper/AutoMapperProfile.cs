using AutoMapper;
using Iis.MaterialDistributor.DataModel.Entities;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<VariableCoefficientEntity, VariableCoefficient>();
        }
    }
}