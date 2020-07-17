﻿using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Iis.Api.GraphQL.Roles;
using IIS.Core.GraphQL.Roles;
using IIS.Core.GraphQL.Users;
using IIS.Core.GraphQL.Materials;
using IIS.Core.GraphQL.Themes;
using Iis.DataModel;
using Iis.DataModel.Elastic;
using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using Iis.DataModel.Themes;
using Iis.Domain.Materials;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;
using Iis.Interfaces.Ontology;
using System.Security.Cryptography;

namespace Iis.Api
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
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

            CreateMap<Iis.Domain.Materials.Material, IIS.Core.GraphQL.Materials.Material>()
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data.ToObject<IEnumerable<IIS.Core.GraphQL.Materials.Data>>()))
                .ForMember(dest => dest.FileId, opts => opts.MapFrom(src => src.File == null ? (Guid?)null : src.File.Id))
                .ForMember(dest => dest.Transcriptions, opts => opts.MapFrom(src => src.Infos.Select(info => info.Data)))
                .ForMember(dest => dest.Children, opts => opts.MapFrom(src => src.Children))
                .ForMember(dest => dest.Highlight, opts => opts.Ignore())
                .ForMember(dest => dest.CreatedDate, opts => opts.MapFrom(src => src.CreatedDate.ToString("MM/dd/yyyy HH:mm:ss")))
                .AfterMap((src, dest, context) => { context.Mapper.Map(src.LoadData, dest); });

            CreateMap<Iis.Domain.Materials.MaterialFeature, MaterialFeatureEntity>();
            CreateMap<Iis.Domain.Materials.MaterialInfo, MaterialInfoEntity>()
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data.ToString()))
                .ForMember(dest => dest.MaterialFeatures, opts => opts.MapFrom(src => src.Features));

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

            CreateMap<UserEntity, IIS.Core.GraphQL.Users.User>();

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

            CreateMap<IIisElasticField, ElasticFieldEntity>();
            CreateMap<IIisElasticField, Iis.Domain.Elastic.IisElasticField>();
            CreateMap<IIisElasticField, IIS.Core.GraphQL.ElasticConfig.ElasticField>();
            CreateMap<Iis.Domain.Materials.Material, Iis.DataModel.Materials.MaterialEntity>()
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

            CreateMap<MaterialEntity, Iis.Domain.Materials.Material>()
                .ForMember(dest => dest.File, opts => {
                    opts.PreCondition(src => (src.FileId.HasValue));
                    opts.MapFrom(src => new FileInfo(src.FileId.Value));
                })
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => src.Metadata == null ? null : JObject.Parse(src.Metadata)))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data == null ? null : JArray.Parse(src.Data)))
                .ForMember(dest => dest.Importance, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<Domain.Materials.MaterialSign>(MaterialEntity.Importance)))
                .ForMember(dest => dest.Reliability, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<Domain.Materials.MaterialSign>(MaterialEntity.Reliability)))
                .ForMember(dest => dest.Relevance, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<Domain.Materials.MaterialSign>(MaterialEntity.Relevance)))
                .ForMember(dest => dest.Completeness, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<Domain.Materials.MaterialSign>(MaterialEntity.Completeness)))
                .ForMember(dest => dest.SourceReliability, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<Domain.Materials.MaterialSign>(MaterialEntity.SourceReliability)))
                .ForMember(dest => dest.ProcessedStatus, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<Domain.Materials.MaterialSign>(MaterialEntity.ProcessedStatus)))
                .ForMember(dest => dest.SessionPriority, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<Domain.Materials.MaterialSign>(MaterialEntity.SessionPriority)))
                .ForMember(dest => dest.LoadData, opts => opts.MapFrom(src => Domain.Materials.MaterialLoadData.MapLoadData(src.LoadData)));

            CreateMap<MaterialInput, Iis.Domain.Materials.Material>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => JObject.Parse(src.Metadata)))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data == null ? null : JArray.FromObject(src.Data)))
                .ForMember(dest => dest.File, opts => opts.MapFrom(src => src.FileId.HasValue ? new FileInfo((Guid)src.FileId): null ))
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

            CreateMap<UserEntity, DbLayer.Repositories.Assignee>();
            CreateMap<MaterialSignEntity, DbLayer.Repositories.MaterialSign>();
            CreateMap<MaterialEntity, DbLayer.Repositories.MaterialDocument>()
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => src.Metadata == null ? null : JObject.Parse(src.Metadata)))
                .ForMember(dest => dest.Importance, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<DbLayer.Repositories.MaterialSign>(MaterialEntity.Importance)))
                .ForMember(dest => dest.Reliability, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<DbLayer.Repositories.MaterialSign>(MaterialEntity.Reliability)))
                .ForMember(dest => dest.Relevance, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<DbLayer.Repositories.MaterialSign>(MaterialEntity.Relevance)))
                .ForMember(dest => dest.Completeness, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<DbLayer.Repositories.MaterialSign>(MaterialEntity.Completeness)))
                .ForMember(dest => dest.SourceReliability, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<DbLayer.Repositories.MaterialSign>(MaterialEntity.SourceReliability)))
                .ForMember(dest => dest.ProcessedStatus, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<DbLayer.Repositories.MaterialSign>(MaterialEntity.ProcessedStatus)))
                .ForMember(dest => dest.SessionPriority, src => src.MapFrom((MaterialEntity, Material, MaterialSign, context) =>
                    context.Mapper.Map<DbLayer.Repositories.MaterialSign>(MaterialEntity.SessionPriority)))
                .ForMember(dest => dest.LoadData, opts =>
                    opts.MapFrom(src => JsonConvert.DeserializeObject<DbLayer.Repositories.MaterialLoadData>(src.LoadData)))
                .ForMember(dest => dest.Data, opts =>
                    opts.MapFrom(src => src.Data == null ? null : JsonConvert.DeserializeObject<Iis.DbLayer.Repositories.Data[]>(src.Data)));

            CreateMap<DbLayer.Repositories.Assignee, Iis.Roles.User>();                
            CreateMap<DbLayer.Repositories.MaterialLoadData, Iis.Domain.Materials.MaterialLoadData>();
            CreateMap<DbLayer.Repositories.MaterialSign, Iis.Domain.Materials.MaterialSign>();
            CreateMap<DbLayer.Repositories.MaterialDocument, Iis.Domain.Materials.Material>()
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data == null ? null: JArray.FromObject(src.Data)))
                .ForMember(dest => dest.Children, opts => opts.Ignore())
                .ForMember(dest => dest.Assignee, opts => opts.MapFrom(src => src.Assignee));

            CreateMap<IChangeHistoryItem, Iis.DataModel.ChangeHistory.ChangeHistoryEntity>();
            CreateMap<IChangeHistoryItem, IIS.Core.GraphQL.ChangeHistory.ChangeHistoryItem>();

            //mapping: GraphQl.UserInput -> Roles.User
            CreateMap<BaseUserInput, Iis.Roles.User>()
                .ForMember(dest => dest.Roles, opts=> opts.MapFrom(src => src.Roles.Select(id =>  new Iis.Roles.Role{ Id = id})));
            CreateMap<UserCreateInput, Iis.Roles.User>()
                .IncludeBase<BaseUserInput, Iis.Roles.User>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()));
            CreateMap<UserUpdateInput, Iis.Roles.User>()
                .IncludeBase<BaseUserInput, Iis.Roles.User>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opts => opts.Ignore());

            //mapping: Roles.User -> GraphQl.User
            CreateMap<Iis.Roles.User, User>();

            //mappring: UserEntity -> Roles.User
            CreateMap<UserEntity, Roles.User>()
                .ForMember(dest => dest.Roles, opts => opts.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));

            //mapping: Roles.User -> UserEntity
            CreateMap<Roles.User, UserEntity>();

            CreateMap<UserEntity, UserEntity>()
                .ForMember(dest => dest.Username, opts => opts.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, sourceValue, targetValue) => sourceValue != null));

            CreateMap<Iis.Domain.Materials.MaterialsCountByType, IIS.Core.GraphQL.Materials.MaterialsCountByType>();

            //theme: graphQl input -> domain
            CreateMap<IIS.Core.GraphQL.Themes.ThemeInput, Iis.ThemeManagement.Models.Theme>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.User, opts => opts.MapFrom(src => new Iis.Roles.User{ Id = src.UserId.Value }));

            CreateMap<IIS.Core.GraphQL.Themes.UpdateThemeInput, Iis.ThemeManagement.Models.Theme>()
                .ForMember(dest => dest.User, opts => opts.MapFrom(src =>
                    src.UserId.HasValue ? new Iis.Roles.User { Id = src.UserId.Value } : null));

            // theme: domain -> entity
            CreateMap<Iis.ThemeManagement.Models.Theme, ThemeEntity>()
                .ForMember(dest => dest.User, opts => opts.Ignore())
                .ForMember(dest => dest.Type, opts => opts.Ignore());

            // theme: entity -> domain
            CreateMap<ThemeEntity, Iis.ThemeManagement.Models.Theme>();

            //theme: domain -> graphQl
            CreateMap<Iis.ThemeManagement.Models.Theme, Theme>();

            // themeType: domain -> entity
            CreateMap<Iis.ThemeManagement.Models.ThemeType, ThemeTypeEntity>();

            // themeType: entity -> domain
            CreateMap<ThemeTypeEntity, Iis.ThemeManagement.Models.ThemeType>();

            //theme: domain -> graphQl
            CreateMap<Iis.ThemeManagement.Models.ThemeType, ThemeType>();

            CreateMap<IIS.Core.GraphQL.ML.MachineLearningHadnlersCountInput, IIS.Core.GraphQL.ML.MachineLearningHadnlersCountResult>();
        }
    }
}
