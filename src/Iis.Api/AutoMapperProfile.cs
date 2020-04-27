using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Iis.Api.GraphQL.Roles;
using Iis.Api.Ontology.Migration;
using IIS.Core.GraphQL.Roles;
using IIS.Core.GraphQL.Materials;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.DataModel.Materials;
using Iis.Domain.Materials;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Materials;


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
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data.ToString()))
                .ForMember(dest => dest.MaterialFeatures, opts => opts.Ignore());

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

            CreateMap<Iis.Domain.MachineLearning.MlProcessingResult, IIS.Core.GraphQL.ML.MlProcessingResult>();

            CreateMap<MLResponseEntity, Iis.Domain.MachineLearning.MlProcessingResult>()
                .ForMember(dest => dest.MlHandlerName, opts => opts.MapFrom(src => src.MLHandlerName))
                .ForMember(dest => dest.ResponseText, opts => opts.MapFrom(src => src.OriginalResponse));

            CreateMap<IIS.Core.GraphQL.ML.MachineLearningResponseInput,Iis.Domain.MachineLearning.MlResponse>();

            CreateMap<Iis.Domain.MachineLearning.MlResponse, Iis.DataModel.Materials.MLResponseEntity>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(o => Guid.NewGuid()))
                .ForMember(dest => dest.MLHandlerName, opts => opts.MapFrom(src => src.HandlerName));

            CreateMap<Iis.DataModel.Materials.MLResponseEntity, Iis.Domain.MachineLearning.MlResponse>()
                .ForMember(dest => dest.HandlerName, opts => opts.MapFrom(src => src.MLHandlerName));

            CreateMap<Iis.Domain.MachineLearning.MlResponse, IIS.Core.GraphQL.ML.MachineLearningResult>();
            
            CreateMap<IIS.Core.GraphQL.NodeMaterialRelation.NodeMaterialRelationInput, IIS.Core.NodeMaterialRelation.NodeMaterialRelation>();

            CreateMap<Iis.Domain.Materials.Material, Iis.DataModel.Materials.MaterialEntity>()
                .ForMember(dest => dest.File, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata == null ? (string) null: src.Metadata.ToString(Formatting.None)))
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data == null ? (string) null : src.Data.ToString(Formatting.None)))
                .ForMember(dest => dest.LoadData, opt => opt.MapFrom(src => src.LoadData == null? (string) null : src.LoadData.ToJson()));
        }
    }
}
