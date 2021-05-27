using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

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

        public async Task<FileIdDto> IsDuplicatedAsync(byte[] contents)
        {
            var hash = ComputeHash(contents);

            return await IsDuplicatedAsync(contents, hash);
        }

        public async Task<FileIdDto> IsDuplicatedAsync(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                stream.Seek(0, SeekOrigin.Begin);
                var contents = ms.ToArray();
                var hash = ComputeHash(contents);
                return await IsDuplicatedAsync(contents, hash);
            }            
        }

        public async Task<FileIdDto> SaveFileAsync(Stream stream, string fileName, string contentType,
            CancellationToken token)
        {
            var contents = await ConvertToBytes(stream, token);
            var hash = ComputeHash(contents);

            var duplicatedResult = await IsDuplicatedAsync(contents, hash, token);
            if (duplicatedResult.IsDuplicate)
                return duplicatedResult;

            var file = new FileEntity
            {
                Name = fileName,
                ContentType = contentType,
                ContentHash = hash,
                UploadTime = DateTime.Now, // todo: move to db or to datetime provider
                IsTemporary = true,
            };

            if (_configuration.Storage == Storage.Database || string.IsNullOrEmpty(_configuration.Path))
            {
                file.Contents = contents;
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
                await File.WriteAllBytesAsync(path, contents);
            }

            return new FileIdDto
            {
                Id = file.Id
            };
        }

        public async Task<Iis.Domain.Materials.File> GetFileAsync(Guid id)
        {
            var file = await RunWithoutCommitAsync(uow => uow.FileRepository.GetAsync(f => f.Id == id));
            if (file == null) return null;

            byte[] contents;

            if (_configuration.Storage == Storage.Folder && !string.IsNullOrEmpty(_configuration.Path))
            {
                string path = Path.Combine(_configuration.Path, file.Id.ToString("D"));
                if (File.Exists(path))
                {
                    contents = await File.ReadAllBytesAsync(path);
                }
                else
                {
                    contents = file.Contents;
                }
            }
            else
            {
                contents = file.Contents;
            }

            if (contents == null) return null;

            var ms = new MemoryStream(contents);
            return new Iis.Domain.Materials.File(id, file.Name, file.ContentType, ms, file.IsTemporary);
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

        private async Task<FileIdDto> IsDuplicatedAsync(byte[] contents, Guid hash, CancellationToken token = default)
        {
            var files = await RunWithoutCommitAsync(uow => uow.FileRepository.GetManyAsync(f => f.ContentHash == hash));
            foreach (var fileData in files)
            {
                var storedFileBody = Array.Empty<byte>();

                //since we have at least 2 types of storage: Database and Folder 
                //let's do checks in both of them  
                if (fileData.Contents != null)
                {
                    storedFileBody = fileData.Contents;
                }
                else if (_configuration.Storage == Storage.Folder)
                {
                    var storedFilePath = Path.Combine(_configuration.Path, fileData.Id.ToString("D"));

                    var storedFileInfo = new FileInfo(storedFilePath);

                    if (storedFileInfo.Exists)
                    {
                        storedFileBody = await File.ReadAllBytesAsync(storedFileInfo.FullName, token);
                    }
                }

                if (storedFileBody.Length != contents.Length)
                {
                    continue;
                }

                if (contents.SequenceEqual(storedFileBody))
                {
                    return new FileIdDto
                    {
                        Id = fileData.Id,
                        IsDuplicate = true
                    };
                }
            }

            return new FileIdDto
            {
                IsDuplicate = false
            };
        }

        private static Guid ComputeHash(byte[] data)
        {
            using HashAlgorithm algorithm = MD5.Create();
            byte[] bytes = algorithm.ComputeHash(data);
            return new Guid(bytes);
        }

        private static async Task<byte[]> ConvertToBytes(Stream stream, CancellationToken token)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, token);
            return ms.ToArray();
        }
    }
}