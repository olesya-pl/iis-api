using AutoMapper;
using Iis.Api.GraphQL.Roles;
using Iis.DataModel;
using Iis.DataModel.Elastic;
using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using Iis.DataModel.Themes;
using Iis.DataModel.Annotations;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Roles;
using Iis.Services.Contracts;
using IIS.Core.GraphQL.Materials;
using IIS.Core.GraphQL.Roles;
using IIS.Core.GraphQL.Themes;
using IIS.Core.GraphQL.Users;
using Iis.Api.GraphQL.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Params;
using Role = Iis.Services.Contracts.Role;
using User = IIS.Core.GraphQL.Users.User;
using Iis.Interfaces.Ontology.Data;
using Contracts = Iis.Services.Contracts;
using Iis.DataModel.Reports;
using Iis.Events.Reports;
using Iis.Api.GraphQL.Aliases;
using Iis.DataModel.ChangeHistory;

namespace Iis.Api
{
    public class AutoMapperProfile: Profile
    {
        private const string Iso8601DateFormat = "yyyy-MM-dd'T'HH:mm:ssZ";
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
                .ForMember(dest => dest.AccessLevel, opts => opts.MapFrom(src => (byte)src.AccessLevel))
                .AfterMap((src, dest, context) => { context.Mapper.Map(src.LoadData, dest); });

            CreateMap<Iis.Domain.Materials.MaterialFeature, MaterialFeatureEntity>();
            CreateMap<Iis.Domain.Materials.MaterialInfo, MaterialInfoEntity>()
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src.Data.ToString()))
                .ForMember(dest => dest.MaterialFeatures, opts => opts.MapFrom(src => src.Features));

            CreateMap<RoleAccessEntity, AccessGranted>()
                .ForMember(dest => dest.Kind, opts => opts.MapFrom(src => src.AccessObject.Kind))
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.AccessObject.Category))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.AccessObject.Title))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.AccessObjectId));
            CreateMap<AccessGranted, RoleAccessEntity>()
                .ForMember(dest => dest.AccessObjectId, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()));
            CreateMap<AccessGranted, AccessObjectEntity>();
            CreateMap<RoleEntity, Role>()
                .ForMember(dest => dest.AccessGrantedItems, opts => opts.MapFrom(src => src.RoleAccessEntities))
                .ForMember(dest => dest.ActiveDirectoryGroupIds, opts => opts.MapFrom(src => src.RoleGroups.Select(r => r.GroupId)));
            CreateMap<Role, RoleEntity>()
                .ForMember(dest => dest.RoleAccessEntities, opts => opts.MapFrom(src => new List<RoleAccessEntity>()));
            CreateMap<AccessGranted, AccessTab>()
                .ForMember(dest => dest.Visible, opts => opts.MapFrom(src => src.ReadGranted));
            CreateMap<AccessTab, AccessGranted>()
                .ForMember(dest => dest.ReadGranted, opts => opts.MapFrom(src => src.Visible))
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => AccessCategory.Tab));
            CreateMap<AccessGranted, AccessEntity>();
            CreateMap<AccessEntity, AccessGranted>()
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => AccessCategory.Entity))
                .ForMember(dest => dest.ReadGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(AccessGranted.ReadAccessName)))
                .ForMember(dest => dest.CreateGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(AccessGranted.CreateAccessName)))
                .ForMember(dest => dest.UpdateGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(AccessGranted.UpdateAccessName)))
                .ForMember(dest => dest.DeleteGranted, opts => opts
                    .MapFrom(src => src.AllowedOperations.Contains(AccessGranted.DeleteAccessName)));
            CreateMap<ActiveDirectoryGroupDto, Group>();

            CreateMap<Role, IIS.Core.GraphQL.Roles.Role>()
                .ForMember(dest => dest.ActiveDirectoryGroupIds, opts => opts.MapFrom(src => src.ActiveDirectoryGroupIds.Select(g => g.ToString("N"))));
            CreateMap<CreateRoleModel, Role>()
                .ForMember(dest => dest.Tabs, opts => opts.Ignore())
                .ForMember(dest => dest.Entities, opts => opts.Ignore())
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()));

            CreateMap<UpdateRoleModel, Role>()
            .ForMember(dest => dest.Tabs, opts => opts.Ignore())
            .ForMember(dest => dest.Entities, opts => opts.Ignore());

            CreateMap<UserEntity, IIS.Core.GraphQL.Users.User>();

            CreateMap<IIS.Core.GraphQL.ML.MachineLearningResponseInput,Iis.Domain.MachineLearning.MLResponse>()
                .ForMember(dest => dest.ProcessingDate, opts => opts.MapFrom(src => DateTime.Now));

            CreateMap<Iis.Domain.MachineLearning.MLResponse, Iis.DataModel.Materials.MLResponseEntity>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(o => Guid.NewGuid()));

            CreateMap<Iis.DataModel.Materials.MLResponseEntity, Iis.Domain.MachineLearning.MLResponse>();

            CreateMap<Iis.Domain.MachineLearning.MLResponse, IIS.Core.GraphQL.ML.MachineLearningResult>()
                .ForMember(dest => dest.ProcessingDateTime, opts => opts.MapFrom(src => src.ProcessingDate.ToString("MM/dd/yyyy HH:mm:ss")));

            CreateMap<IIS.Core.GraphQL.NodeMaterialRelation.NodeMaterialRelationInput, IIS.Core.NodeMaterialRelation.NodeMaterialRelation>();

            CreateMap<IIisElasticField, ElasticFieldEntity>();
            CreateMap<IIisElasticField, Iis.Domain.Elastic.IisElasticField>();
            CreateMap<IIisElasticField, IIS.Core.GraphQL.ElasticConfig.ElasticField>();
            CreateMap<ElasticFieldEntity, IIS.Core.GraphQL.ElasticConfig.ElasticField>()
                .ForMember(dest => dest.IsAggregated, opts => opts.MapFrom(src => false));

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
                    opts.MapFrom(src => new FileDto(src.FileId.Value));
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

            CreateMap<UserEntity, DbLayer.Repositories.Assignee>();
            CreateMap<MaterialSignEntity, DbLayer.Repositories.MaterialSign>();
            CreateMap<MaterialEntity, DbLayer.Repositories.MaterialDocument>()
                .ForMember(dest => dest.CreatedDate, opts => opts.MapFrom(src => src.CreatedDate.ToString(Iso8601DateFormat, CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.Metadata, opts => opts.MapFrom(src => src.Metadata == null ? null : JObject.Parse(src.Metadata)))
                .ForMember(dest => dest.FileName, opts => opts.MapFrom(src => src.File == null ? null : src.File.Name))
                .ForPath(dest => dest.SecurityAttributes.AccessLevel, opts => opts.MapFrom(src=> src.AccessLevel))
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
                    opts.MapFrom(src => JsonConvert.DeserializeObject<DbLayer.Repositories.MaterialLoadData>(src.LoadData)));

            CreateMap<DbLayer.Repositories.Assignee, Services.Contracts.User>();
            CreateMap<DbLayer.Repositories.MaterialLoadData, Iis.Domain.Materials.MaterialLoadData>();
            CreateMap<DbLayer.Repositories.MaterialSign, Iis.Domain.Materials.MaterialSign>();
            CreateMap<DbLayer.Repositories.MaterialDocument, Iis.Domain.Materials.Material>()
                .ForMember(dest => dest.File, opts => opts.MapFrom(src => src.FileId.HasValue ? new FileDto(src.FileId.Value): null))
                .ForMember(dest => dest.CreatedDate, opts => opts.MapFrom(src => DateTime.ParseExact(src.CreatedDate, Iso8601DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)))
                .ForMember(dest => dest.Children, opts => opts.Ignore())
                .ForMember(dest => dest.Assignee, opts => opts.MapFrom(src => src.Assignee));
            

            //mapping: GraphQl.UserInput -> Roles.User
            CreateMap<BaseUserInput, Services.Contracts.User>()
                .ForMember(dest => dest.Roles, opts=> opts.MapFrom(src => src.Roles.Select(id =>  new Role{ Id = id})));
            CreateMap<UserCreateInput, Services.Contracts.User>()
                .IncludeBase<BaseUserInput, Services.Contracts.User>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()));
            CreateMap<UserUpdateInput, Services.Contracts.User>()
                .IncludeBase<BaseUserInput, Services.Contracts.User>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opts => opts.Ignore());

            //mapping: Roles.User -> GraphQl.User
            CreateMap<Services.Contracts.User, User>();

            //mappring: UserEntity -> Roles.User
            CreateMap<UserEntity, Services.Contracts.User>()
                .ForMember(dest => dest.Roles, opts => opts.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));

            //mapping: Roles.User -> UserEntity
            CreateMap<Services.Contracts.User, UserEntity>();

            CreateMap<UserEntity, UserEntity>()
                .ForMember(dest => dest.Username, opts => opts.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, sourceValue, targetValue) => sourceValue != null));

            CreateMap<Iis.Domain.Materials.MaterialsCountByType, IIS.Core.GraphQL.Materials.MaterialsCountByType>();

            //theme: graphQl input -> domain
            CreateMap<IIS.Core.GraphQL.Themes.ThemeInput, ThemeDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.User, opts => opts.MapFrom(src => new Services.Contracts.User{ Id = src.UserId.Value }))
                .ForMember(dest => dest.UpdatedAt, opts => opts.MapFrom(src => DateTime.UtcNow));

            CreateMap<IIS.Core.GraphQL.Themes.UpdateThemeInput, ThemeDto>()
                .ForMember(dest => dest.User, opts => opts.MapFrom(src =>
                    src.UserId.HasValue ? new Services.Contracts.User { Id = src.UserId.Value } : null))
                .ForMember(dest => dest.UpdatedAt, opts => opts.MapFrom(src => DateTime.UtcNow));

            // theme: domain -> entity
            CreateMap<ThemeDto, ThemeEntity>()
                .ForMember(dest => dest.User, opts => opts.Ignore())
                .ForMember(dest => dest.Type, opts => opts.Ignore());

            // theme: entity -> domain
            CreateMap<ThemeEntity, ThemeDto>();

            //theme: domain -> graphQl
            CreateMap<ThemeDto, Theme>();

            // themeType: domain -> entity
            CreateMap<ThemeTypeDto, ThemeTypeEntity>();

            // themeType: entity -> domain
            CreateMap<ThemeTypeEntity, ThemeTypeDto>();

            //theme: domain -> graphQl
            CreateMap<ThemeTypeDto, ThemeType>();

            CreateMap<IIS.Core.GraphQL.ML.MachineLearningHadnlersCountInput, IIS.Core.GraphQL.ML.MachineLearningHadnlersCountResult>();

            CreateMap<INodeBase, NodeEntity>();
            CreateMap<IRelationBase, RelationEntity>();
            CreateMap<IAttributeBase, AttributeEntity>();
            //annotations: graph ql input -> domain
            CreateMap<IIS.Core.GraphQL.Annotations.AnnotationInput, Contracts.Annotations.Annotation>();

            //annotations: domain -> graph ql
            CreateMap<Contracts.Annotations.Annotation, IIS.Core.GraphQL.Annotations.Annotation>()
                .ForMember(dest => dest.Content, opts => opts.MapFrom(src => string.IsNullOrWhiteSpace(src.Content) ? null : JObject.Parse(src.Content)));

            //annotations: domain -> entity
            CreateMap<Contracts.Annotations.Annotation, AnnotationEntity>();

            //annotations: entity -> domain
            CreateMap<AnnotationEntity, Contracts.Annotations.Annotation>();

            CreateMap<Iis.Domain.IncomingRelation, Iis.Api.Ontology.IncomingRelation>()
                .ForMember(dest => dest.Entity, opts => opts.Ignore());

            CreateMap<Iis.Domain.FlightRadar.FlightRadarHistory, Iis.DataModel.FlightRadar.LocationHistoryEntity>();
            CreateMap<Iis.FlightRadar.DataModel.Routes, Iis.Domain.FlightRadar.FlightRadarHistory>()
                .ForMember(dest => dest.Lat, opts => opts.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Long, opts => opts.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.RegisteredAt, opts => opts.MapFrom(src => src.TimeNow))
                .ForMember(dest => dest.ExternalId, opts => opts.MapFrom(src => src.Id.ToString()));


            #region Reports

            CreateMap<ReportEntity, ReportDto>()
                .ForMember(dest => dest.ReportEventIds, opts => opts.MapFrom(src => src.ReportEvents.Select(e => e.EventId)));

            CreateMap<ReportDto, ReportEntity>()
                .ForMember(dest => dest.ReportEvents, opts => opts.Ignore());

            CreateMap<ReportEntity, ReportEvent>()
                .ForMember(dest => dest.ReportEventIds, opts => opts.MapFrom(src => src.ReportEvents.Select(r => r.EventId)))
                .IncludeAllDerived();
            CreateMap<ReportEntity, ReportCreatedEvent>();
            CreateMap<ReportEntity, ReportUpdatedEvent>();
            CreateMap<ReportEntity, ReportRemovedEvent>();


            CreateMap<ReportCreatedEvent, ReportDto>();
            CreateMap<ReportUpdatedEvent, ReportDto>();
            CreateMap<ReportRemovedEvent, ReportDto>();

            #endregion

            #region Aliases

            CreateMap<AliasEntity, AliasDto>();
            CreateMap<AliasDto, AliasEntity>();

            CreateMap<Alias, AliasDto>();
            CreateMap<AliasDto, Alias>();

            #endregion

            CreateMap<ChangeHistoryEntity, ChangeHistoryDto>().ReverseMap();
            CreateMap<ChangeHistoryDto, IIS.Core.GraphQL.ChangeHistory.ChangeHistoryItem>()
                .ForMember(dest => dest.OldValue, opts => opts.MapFrom(src => src.OldTitle ?? src.OldValue))
                .ForMember(dest => dest.NewValue, opts => opts.MapFrom(src => src.NewTitle ?? src.NewValue));

            CreateMap<Iis.Interfaces.Elastic.AggregationBucket, IIS.Core.GraphQL.Entities.AggregationBucket>();
            CreateMap<Iis.Interfaces.Elastic.AggregationItem, IIS.Core.GraphQL.Entities.AggregationItem>();
            CreateMap<Iis.Interfaces.Elastic.SearchEntitiesByConfiguredFieldsResult, IIS.Core.GraphQL.Entities.OntologyFilterableQueryResponse>()
                .ForMember(dest => dest.Aggregations, opts => opts.MapFrom(src => src.Aggregations))
                .ForMember(dest => dest.Items, opts => opts.MapFrom(src => src.Entities));

            CreateMap<SortingInput, SortingParams>();
        }
    }
}
