using AutoMapper;
using Iis.Api.Ontology.Migration;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using Iis.Interfaces.Materials;
using Iis.Interfaces.Roles;
using IIS.Core.GraphQL.Roles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Api
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<NodeEntity, SnapshotNode>().ReverseMap();
            CreateMap<RelationEntity, SnapshotRelation>().ReverseMap();
            CreateMap<AttributeEntity, SnapshotAttribute>().ReverseMap();
            CreateMap<RelationTypeEntity, SnatshotRelationType>().ReverseMap();
            CreateMap<NodeTypeEntity, SnapshotNodeType>().ReverseMap();
            CreateMap<AttributeTypeEntity, SnapshotAttributeType>().ReverseMap();
            CreateMap<IMaterialSignType, MaterialSignTypeEntity>();
            CreateMap<IMaterialSign, MaterialSignEntity>();
            CreateMap<IMaterialSign, Iis.Domain.Materials.MaterialSign>();
            CreateMap<IMaterialSign, IIS.Core.GraphQL.Materials.MaterialSign>();

            CreateMap<MaterialSignEntity, IIS.Core.GraphQL.Materials.MaterialSignFull>()
                .ForMember(dest => dest.TypeName, opts => opts.MapFrom(src => src.MaterialSignType.Name))
                .ForMember(dest => dest.TypeTitle, opts => opts.MapFrom(src => src.MaterialSignType.Title));

            CreateMap<IMaterialLoadData, Iis.Domain.Materials.MaterialLoadData>();
            CreateMap<IMaterialLoadData, IIS.Core.GraphQL.Materials.MaterialLoadData>();
            CreateMap<IMaterialLoadData, IIS.Core.GraphQL.Materials.Material>();

            CreateMap<Iis.Domain.Materials.Material, IIS.Core.GraphQL.Materials.Material>()
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => src.Metadata.ToObject<IIS.Core.GraphQL.Materials.Metadata>()))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data.ToObject<IEnumerable<IIS.Core.GraphQL.Materials.Data>>()))
                .ForMember(dest => dest.FileId, opts => opts.MapFrom(src => src.File == null ? (Guid?)null : src.File.Id))
                .ForMember(dest => dest.Transcriptions, opts => opts.MapFrom(src => src.Infos.Select(info => info.Data)))
                .AfterMap((src, dest, context) => { context.Mapper.Map(src.LoadData, dest); });

            CreateMap<Iis.Domain.Materials.MaterialFeature, MaterialFeatureEntity>();
            CreateMap<Iis.Domain.Materials.MaterialInfo, MaterialInfoEntity>()
                .ForMember(dest => dest.MaterialFeatures, opts => opts.MapFrom(src => src.Features));

            CreateMap<Iis.Domain.Materials.Material, MaterialEntity>()
                .ForMember(dest => dest.File, opts => opts.Ignore())
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => src.Metadata.ToString()))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data == null ? null : src.Data.ToString()))
                .ForMember(dest => dest.LoadData, opts => opts.MapFrom(src => src.LoadData.ToJson()))
                .ForMember(dest => dest.MaterialInfos, opts => opts.MapFrom(src => src.Infos));

            CreateMap<RoleAccessEntity, Iis.Roles.AccessGranted>()
                .ForMember(dest => dest.Kind, opts => opts.MapFrom(src => src.AccessObject.Kind))
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.AccessObject.Category))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.AccessObject.Title)); 
            CreateMap<RoleEntity, Iis.Roles.Role>()
                .ForMember(dest => dest.AccessGrantedItems, opts => opts.MapFrom(src => src.RoleAccessEntities));
            CreateMap<Roles.AccessGranted, AccessTab>()
                .ForMember(dest => dest.Visible, opts => opts.MapFrom(src => src.ReadGranted));
            CreateMap<Roles.AccessGranted, AccessEntity>();
            CreateMap<Roles.Role, IIS.Core.GraphQL.Roles.Role>();

            CreateMap<UserEntity, Roles.User>();
            CreateMap<UserEntity, IIS.Core.GraphQL.Users.User>();
            CreateMap<Roles.User, IIS.Core.GraphQL.Users.User>();
        }
    }
}
