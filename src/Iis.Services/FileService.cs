using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility;
using DomainMaterials = Iis.Domain.Materials;

namespace Iis.Services
{
    public class FileService<TUnitOfWork> : BaseService<TUnitOfWork>, IFileService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly FilesConfiguration _configuration;
        private readonly ILogger _logger;

        public FileService(
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            FilesConfiguration configuration,
            ILogger<FileService<IIISUnitOfWork>> logger)
            : base(unitOfWorkFactory)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FileResult> IsDuplicatedAsync(Stream stream)
        {
            var hash = stream.ComputeHashAsGuid();
            return await IsDuplicatedAsync(stream, hash);
        }

        public async Task<FileResult> SaveFileAsync(Stream stream, string fileName, string contentType,
            CancellationToken token)
        {
            var duplicatedResult = await IsDuplicatedAsync(stream);
            if (duplicatedResult.IsDuplicate)
                return duplicatedResult;

            var file = new FileEntity
            {
                Name = fileName,
                ContentType = contentType,
                ContentHash = duplicatedResult.FileHash,
                UploadTime = DateTime.UtcNow,
                IsTemporary = true,
            };

            if (_configuration.Storage == Storage.Database || string.IsNullOrEmpty(_configuration.Path))
            {
                file.Contents = await stream.ToByteArrayAsync(token);
            }

            await RunAsync(uow => uow.FileRepository.Create(file));

            if (_configuration.Storage == Storage.Folder && !string.IsNullOrEmpty(_configuration.Path))
            {
                if (!Directory.Exists(_configuration.Path))
                {
                    try
                    {
                        Directory.CreateDirectory(_configuration.Path);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to create files folder.");
                    }
                }

                string path = Path.Combine(_configuration.Path, file.Id.ToString("D"));
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
            }

            return new FileResult
            {
                Id = file.Id,
                IsDuplicate = false,
                FileHash = duplicatedResult.FileHash
            };
        }

        public async Task<DomainMaterials.File> GetFileAsync(Guid id)
        {
            var file = await RunWithoutCommitAsync(uow => uow.FileRepository.GetAsync(f => f.Id == id));

            if (file == null) return null;

            return new DomainMaterials.File(id, file.Name, file.ContentType, file.IsTemporary);
        }

        public async Task<FileContentResult> GetFileContentAsync(Guid id)
        {
            var entity = await RunWithoutCommitAsync(uow => uow.FileRepository.GetAsync(e => e.Id == id));

            if(entity is null) return FileContentResult.Empty;

            if(_configuration.Storage == Storage.Database && entity.Contents.IsNotEmpty())
            {
                return new FileContentResult
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    ContentType = entity.ContentType,
                    Content = new MemoryStream(entity.Contents)
                };
            }

            if(_configuration.Storage == Storage.Folder && !string.IsNullOrWhiteSpace(_configuration.Path))
            {
                var fileName = Path.Combine(_configuration.Path, entity.Id.ToString("D"));

                if(File.Exists(fileName))
                {
                    return new FileContentResult
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        ContentType = entity.ContentType,
                        Content = File.OpenRead(fileName)
                    };
                }
                else if(entity.Contents.IsNotEmpty())
                {
                    return new FileContentResult
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        ContentType = entity.ContentType,
                        Content = new MemoryStream(entity.Contents)
                    };
                }

            }

            return FileContentResult.Empty;
        }

        public int RemoveFiles(List<Guid> ids)
        {
            var successOperation = 0;
            if (string.IsNullOrEmpty(_configuration.Path))
                throw new ArgumentException("Configuration.Path can not be null");
            foreach (var id in ids)
            {
                var path = Path.Combine(_configuration.Path, id.ToString("D"));
                if (!File.Exists(path)) 
                    continue;

                if (TryDelete(path))
                    successOperation++;
            }

            return successOperation;
        }

        private static bool TryDelete(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate)
        {
            var files = await RunWithoutCommitAsync(uow =>
                uow.FileRepository.GetManyAsync(f => f.IsTemporary && predicate(f.UploadTime)));

            await RunAsync(uow => uow.FileRepository.RemoveRange(files));
        }

        public async Task MarkFilePermanentAsync(Guid fileId)
        {
            var file = await RunWithoutCommitAsync(uow =>
                uow.FileRepository.GetAsync(f => f.IsTemporary && f.Id == fileId));
            if (file == null)
                throw new ArgumentException($"There is no temporary file with id {fileId}");
            file.IsTemporary = false;

            await RunAsync(uow => uow.FileRepository.Update(file));
        }

        private async Task<FileResult> IsDuplicatedAsync(Stream contents, Guid hash, CancellationToken token = default)
        {
            var files = await RunWithoutCommitAsync(uow => uow.FileRepository.GetManyAsync(f => f.ContentHash == hash));
            foreach (var fileData in files)
            {
                var storedFileBody = Array.Empty<byte>();

                //since we have at least 2 types of storage: Database and Folder
                //let's do checks in both of them
                if (fileData.Contents != null)
                {
                    bool areEqual = contents.IsEqual(fileData.Contents);
                    if (areEqual)
                    {
                        return new FileResult
                        {
                            IsDuplicate = true,
                            FileHash = hash
                        };
                    }
                }
                else if (_configuration.Storage == Storage.Folder)
                {
                    var storedFilePath = Path.Combine(_configuration.Path, fileData.Id.ToString("D"));

                    var storedFileInfo = new FileInfo(storedFilePath);

                    if (storedFileInfo.Exists)
                    {
                        using var fsSource = new FileStream(storedFileInfo.FullName, FileMode.Open, FileAccess.Read);
                        if (fsSource.IsEqual(contents))
                        {
                            return new FileResult
                            {
                                IsDuplicate = true,
                                FileHash = hash,
                                Id = Guid.Parse(storedFileInfo.Name)
                            };
                        }
                    }
                }
            }

            return new FileResult
            {
                IsDuplicate = false,
                FileHash = hash
            };
        }
    }
}