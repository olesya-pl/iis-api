using System;
using AutoMapper;
using Iis.DataModel.ChangeHistory;
using Iis.DataModel.Materials;
using Iis.Interfaces.Materials;
using Iis.Services.Contracts.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialLoader
{
    public class AutoMapperProfile: Profile
    {
        private const string Iso8601DateFormat = "yyyy-MM-dd'T'HH:mm:ssZ";
        public AutoMapperProfile()
        {
            CreateMap<IMaterialSign, MaterialSignEntity>();
            CreateMap<Domain.Materials.Material, MaterialEntity>()
                .ForMember(dest => dest.File, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata == null ? (string)null : src.Metadata.ToString(Formatting.None)))
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data == null ? (string)null : src.Data.ToString(Formatting.None)))
                .ForMember(dest => dest.LoadData, opt => opt.MapFrom(src => src.LoadData == null ? (string)null : src.LoadData.ToJson()))
                .ForMember(dest => dest.Importance, opt => opt.Ignore())
                .ForMember(dest => dest.Reliability, opt => opt.Ignore())
                .ForMember(dest => dest.Relevance, opt => opt.Ignore())
                .ForMember(dest => dest.Completeness, opt => opt.Ignore())
                .ForMember(dest => dest.SourceReliability, opt => opt.Ignore())
                .ForMember(dest => dest.SessionPriority, opt => opt.Ignore());
            
            CreateMap<Domain.Materials.MaterialInfo, MaterialInfoEntity>()
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data.ToString()))
                .ForMember(dest => dest.MaterialFeatures, opts => opts.MapFrom(src => src.Features));
            CreateMap<Domain.Materials.MaterialFeature, MaterialFeatureEntity>();
            
            CreateMap<MaterialInput, Iis.Domain.Materials.Material>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => JObject.Parse(src.Metadata)))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data == null ? null : JArray.FromObject(src.Data)))
                .ForMember(dest => dest.File, opts => opts.MapFrom(src => src.FileId.HasValue ? new FileDto((Guid)src.FileId): null ))
                .ForMember(dest => dest.ParentId, opts => opts.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.CreatedDate,
                    opts => opts.MapFrom(src => !src.CreationDate.HasValue ? DateTime.Now : src.CreationDate))
                .AfterMap((src, dest) => {
                    if (dest.Metadata is null) return;

                    dest.Type = dest.Metadata.GetValue("type", StringComparison.InvariantCultureIgnoreCase)?.Value<string>();
                    dest.Source = dest.Metadata.GetValue("source", StringComparison.InvariantCultureIgnoreCase)?.Value<string>();
                });
            
            CreateMap<MaterialInput, Iis.Domain.Materials.MaterialLoadData>()
                .ForMember(dest => dest.From, opts => opts.MapFrom(src => src.From))
                .ForMember(dest => dest.LoadedBy, opts => opts.MapFrom(src => src.LoadedBy))
                .ForMember(dest => dest.Coordinates, opts => opts.MapFrom(src => src.Coordinates))
                .ForMember(dest => dest.Code, opts => opts.MapFrom(src => src.Code))
                .ForMember(dest => dest.ReceivingDate, opts => opts.MapFrom(src => src.CreationDate))
                .ForMember(dest => dest.Objects, opts => opts.MapFrom(src => src.Objects))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags))
                .ForMember(dest => dest.States, opts => opts.MapFrom(src => src.States));
            
            CreateMap<MaterialSignEntity, DbLayer.Repositories.MaterialSign>();
            CreateMap<ChangeHistoryEntity, ChangeHistoryDto>().ReverseMap();
        }
    }
}
