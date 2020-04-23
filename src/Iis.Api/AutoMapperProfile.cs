using AutoMapper;
using Iis.Api.GraphQL.Roles;
using Iis.Api.Ontology.Migration;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using Iis.Domain.Materials;
using Iis.Interfaces.Materials;
using Iis.Interfaces.Roles;
using IIS.Core.GraphQL.Materials;
using IIS.Core.GraphQL.Roles;
using Newtonsoft.Json.Linq;
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

            CreateMap<Iis.Domain.Node, MaterialFeatureNode>()
                .ForMember(dest => dest.NodeTypeId, opts => opts.MapFrom(src => src.Type.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Type.Name))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.Type.Title));
            CreateMap<Iis.Domain.Materials.MaterialFeature, IIS.Core.GraphQL.Materials.MaterialFeature>();
            CreateMap<Iis.Domain.Materials.MaterialInfo, IIS.Core.GraphQL.Materials.MaterialInfo>()
                .ForMember(dest => dest.Features, opts => opts.MapFrom(src => src.Features));
            CreateMap<Iis.Domain.Materials.Material, IIS.Core.GraphQL.Materials.Material>()
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => src.Metadata.ToObject<IIS.Core.GraphQL.Materials.Metadata>()))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data.ToObject<IEnumerable<IIS.Core.GraphQL.Materials.Data>>()))
                .ForMember(dest => dest.FileId, opts => opts.MapFrom(src => src.File == null ? (Guid?)null : src.File.Id))
                .ForMember(dest => dest.Transcriptions, opts => opts.MapFrom(src => src.Infos.Select(info => info.Data)))
                .ForMember(dest => dest.Children, opts => opts.MapFrom(src => src.Children))
                .ForMember(dest => dest.Infos, opts => opts.MapFrom(src => src.Infos))
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

            CreateMap<MaterialEntity, Iis.Domain.Materials.Material>()
                .ForMember(dest => dest.File, opts => opts.Ignore())
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => src.Metadata == null ? null : JObject.Parse(src.Metadata)))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data == null ? null : JArray.FromObject(src.Data)))
                .ForMember(dest => dest.LoadData, opts => opts.MapFrom(src => src.LoadData.ToString()))
                .ForMember(dest => dest.Infos, opts => opts.MapFrom(src => src.MaterialInfos));

            CreateMap<MaterialInput, Iis.Domain.Materials.Material>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.Metadata.Type))
                .ForMember(dest => dest.Source, opts => opts.MapFrom(src => src.Metadata.Source))
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => JObject.FromObject(src.Metadata)))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data == null ? null : JArray.FromObject(src.Data)))
                .ForMember(dest => dest.File, opts => opts.MapFrom(src => new FileInfo((Guid)src.FileId)))
                .ForMember(dest => dest.ParentId, opts => opts.MapFrom(src => src.ParentId));



            CreateMap<RoleAccessEntity, Iis.Roles.AccessGranted>()
                .ForMember(dest => dest.Kind, opts => opts.MapFrom(src => src.AccessObject.Kind))
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.AccessObject.Category))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.AccessObject.Title))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.AccessObjectId));
            CreateMap<Iis.Roles.AccessGranted, RoleAccessEntity>()
                .ForMember(dest => dest.AccessObjectId, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()));
            CreateMap<Iis.Roles.AccessGranted, AccessObjectEntity>();
            CreateMap<RoleEntity, Iis.Roles.Role>()
                .ForMember(dest => dest.AccessGrantedItems, opts => opts.MapFrom(src => src.RoleAccessEntities));
            CreateMap<Roles.Role, RoleEntity>()
                .ForMember(dest => dest.RoleAccessEntities, opts => opts.MapFrom(src => new List<RoleAccessEntity>()));
            CreateMap<Roles.AccessGranted, AccessTab>()
                .ForMember(dest => dest.Visible, opts => opts.MapFrom(src => src.ReadGranted));
            CreateMap<AccessTab, Roles.AccessGranted>()
                .ForMember(dest => dest.ReadGranted, opts => opts.MapFrom(src => src.Visible))
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => AccessCategory.Tab));
            CreateMap<Roles.AccessGranted, AccessEntity>();
            CreateMap<AccessEntity, Roles.AccessGranted>()
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => AccessCategory.Entity))
                .ForMember(dest => dest.ReadGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(Roles.AccessGranted.ReadAccessName)))
                .ForMember(dest => dest.CreateGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(Roles.AccessGranted.CreateAccessName)))
                .ForMember(dest => dest.UpdateGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(Roles.AccessGranted.UpdateAccessName)))
                .ForMember(dest => dest.DeleteGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(Roles.AccessGranted.DeleteAccessName)));

            CreateMap<Roles.Role, IIS.Core.GraphQL.Roles.Role>();
            CreateMap<CreateRoleModel, Roles.Role>()
                .ForMember(dest => dest.Tabs, opts => opts.Ignore())
                .ForMember(dest => dest.Entities, opts => opts.Ignore())
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()));
            CreateMap<UpdateRoleModel, Roles.Role>()
                .ForMember(dest => dest.Tabs, opts => opts.Ignore())
                .ForMember(dest => dest.Entities, opts => opts.Ignore());

            CreateMap<UserEntity, Roles.User>();
            CreateMap<UserEntity, IIS.Core.GraphQL.Users.User>();
            CreateMap<Roles.User, IIS.Core.GraphQL.Users.User>();

            CreateMap<IIS.Core.ML.MlProcessingResult, IIS.Core.GraphQL.ML.MlProcessingResult>();
            CreateMap<MLResponseEntity, IIS.Core.ML.MlProcessingResult>()
                .ForMember(dest => dest.MlHandlerName, opts => opts.MapFrom(src => src.MLHandlerName))
                .ForMember(dest => dest.ResponseText, opts => opts.MapFrom(src => src.OriginalResponse));

            CreateMap<IIS.Core.GraphQL.NodeMaterialRelation.NodeMaterialRelationInput,
                IIS.Core.NodeMaterialRelation.NodeMaterialRelation>();
        }
    }
}
