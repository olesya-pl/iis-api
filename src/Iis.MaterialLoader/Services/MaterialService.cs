using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.Domain.Materials;
using Iis.MaterialLoader.Models;
using Iis.RabbitMq.Channels;
using Iis.Messages.Materials;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Iis.MaterialLoader.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly OntologyContext _dbContext;
        private readonly IPublishMessageChannel<MaterialCreatedMessage> _targetChannel;

        public MaterialService(IFileService fileService,
            IMapper mapper,
            IChangeHistoryService changeHistoryService,
            IPublishMessageChannel<MaterialCreatedMessage> targetChannel,
            OntologyContext dbContext)
        {
            _fileService = fileService;
            _mapper = mapper;
            _changeHistoryService = changeHistoryService;
            _targetChannel = targetChannel;
            _dbContext = dbContext;
        }

        private async Task<Material> SaveAsync(Material material, Guid? changeRequestId = null)
        {
            await MakeFilePermanent(material);
            await ValidateMaterialParent(material);

            var materialEntity = _mapper.Map<MaterialEntity>(material);

            _dbContext.Materials.Add(materialEntity);
            await _dbContext.SaveChangesAsync();

            var requestId = changeRequestId.GetValueOrDefault(Guid.NewGuid());
            await SaveMaterialChangeHistory(material, requestId);
            await SaveMaterialChildren(material, requestId);
            await SaveMaterialInfoEntities(material);

            _targetChannel.Send(CreatedMessage(material));

            return material;
        }

        private MaterialCreatedMessage CreatedMessage(Material material)
        {
            return new MaterialCreatedMessage
            {
                MaterialId = material.Id,
                FileId = material.FileId,
                ParentId = material.ParentId,
                CreatedDate = material.CreatedDate,
                Type = material.Type,
                Source = material.Source
            };
        }

        public Task<Material> SaveAsync(MaterialInput materialInput)
        {
            var material = MapToMaterial(materialInput);
            return SaveAsync(material);
        }

        private Material MapToMaterial(MaterialInput materialInput)
        {
            var material = _mapper.Map<Material>(materialInput);
            material.Reliability = GetMaterialSign(materialInput.ReliabilityText);
            material.SourceReliability = GetMaterialSign(materialInput.SourceReliabilityText);
            material.LoadData = _mapper.Map<MaterialLoadData>(materialInput);

            return material;
        }

        private MaterialSign GetMaterialSign(string signValue)
        {
            if (string.IsNullOrEmpty(signValue))
                return null;

            var entity = _dbContext.MaterialSigns
                .AsNoTracking()
                .Include(x => x.MaterialSignType)
                .FirstOrDefault(x => x.Title == signValue);

            return entity is null ? null : _mapper.Map<MaterialSign>(entity);
        }

        private Task SaveMaterialChangeHistory(Material material, Guid changeRequestId)
        {
            var timeStamp = DateTime.UtcNow;
            var changeItems = new List<ChangeHistoryDto>
            {
                new ChangeHistoryDto
                {
                    Date = timeStamp,
                    NewValue = material.Id.ToString(),
                    PropertyName = nameof(material.Id),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                },
                new ChangeHistoryDto
                {
                    Date = timeStamp,
                    NewValue = material.Source,
                    PropertyName = nameof(material.Source),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                },
                new ChangeHistoryDto
                {
                    Date = timeStamp,
                    NewValue = material.LoadData.LoadedBy,
                    PropertyName = nameof(material.LoadData.LoadedBy),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                },
                new ChangeHistoryDto
                {
                    Date = timeStamp,
                    NewValue = material.AccessLevel.ToString("D"),
                    PropertyName = nameof(material.AccessLevel),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                }
            };
            return _changeHistoryService.SaveMaterialChanges(changeItems);
        }

        private async Task MakeFilePermanent(Material material)
        {
            if (material.HasAttachedFile())
            {
                var file = await _fileService.GetFileAsync(material.File.Id);
                if (file == null) throw new ArgumentException($"File with guid {material.File.Id} was not found");
                if (!file.IsTemporary) throw new ArgumentException($"File with guid {material.File.Id} is already used");

                await _fileService.MarkFilePermanentAsync(file.Id);

                // todo: implement correct file type - material type compatibility checking
                if (material.Type == "cell.voice" && !file.ContentType.StartsWith("audio/"))
                    throw new ArgumentException($"\"{material.Type}\" material expects audio file to be attached. Got \"{file.ContentType}\"");
            }
        }

        private async Task ValidateMaterialParent(Material material)
        {
            if (material.ParentId.HasValue)
            {
                var parentMaterial = await GetMaterialsQuery().SingleOrDefaultAsync(x => x.Id == material.ParentId.Value);
                if (parentMaterial == null)
                    throw new ArgumentException($"Material with guid {material.ParentId.Value} does not exist");
            }
        }

        private async Task SaveMaterialChildren(Material material, Guid changeRequestId)
        {
            foreach (var child in material.Children)
            {
                child.ParentId = material.Id;
                await SaveAsync(child, changeRequestId);
            }
        }

        private Task SaveMaterialInfoEntities(Material material)
        {
            var materialInfoEntities = material.Infos.Select(info => Map(info, material.Id)).ToList();
            _dbContext.MaterialInfos.AddRange(materialInfoEntities);
            return _dbContext.SaveChangesAsync();
        }

        private MaterialInfoEntity Map(MaterialInfo info, Guid materialId)
        {
            return new MaterialInfoEntity
            {
                Id = info.Id,
                MaterialId = materialId,
                Data = info.Data.ToString(),
                Source = info.Source,
                SourceType = info.SourceType,
                SourceVersion = info.SourceVersion,
            };
        }

        private IQueryable<MaterialEntity> GetMaterialsQuery()
        {
            return _dbContext.Materials
                .Include(m => m.File)
                .Include(m => m.Importance)
                .Include(m => m.Reliability)
                .Include(m => m.Relevance)
                .Include(m => m.Completeness)
                .Include(m => m.SourceReliability)
                .Include(m => m.ProcessedStatus)
                .Include(m => m.SessionPriority)
                .Include(m => m.Assignee)
                .AsNoTracking();
        }
    }    
}
