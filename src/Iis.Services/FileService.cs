using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Repository;
using IIS.Repository.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Files.EntityFramework
{
    public class FileService<TUnitOfWork> : BaseService<TUnitOfWork>, IFileService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly FilesConfiguration _configuration;
        private readonly ILogger _logger;

        public FileService(
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory, 
            FilesConfiguration configuration, 
            ILogger<FileService<IIISUnitOfWork>> logger)
            :base(unitOfWorkFactory)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FileIdDto> SaveFileAsync(Stream stream, string fileName, string contentType, CancellationToken token)
        {
            byte[] contents;
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms, token);
                contents = ms.ToArray();
            }

            Guid hash = ComputeHash(contents);

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
                else
                if (_configuration.Storage == Storage.Folder)
                {
                    var storedFilePath = Path.Combine(_configuration.Path, fileData.Id.ToString("D"));

                    var storedFileInfo = new FileInfo(storedFilePath);

                    if (storedFileInfo.Exists)
                    {
                        storedFileBody = await File.ReadAllBytesAsync(storedFileInfo.FullName);
                    }
                }

                if (storedFileBody.Length != contents.Length)
                {
                    continue;
                }

                if (contents.SequenceEqual(storedFileBody))
                {
                    // Duplicate found.
                    return new FileIdDto
                    {
                        Id = fileData.Id,
                        IsDuplicate = true
                    };
                }
            }

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

        public async Task<FileDto> GetFileAsync(Guid id)
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
            return new FileDto(id, file.Name, file.ContentType, ms, file.IsTemporary);
        }

        public async Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate)
        {
            var files = await RunWithoutCommitAsync(uow => uow.FileRepository.GetManyAsync(f => f.IsTemporary && predicate(f.UploadTime)));

            await RunAsync(uow => uow.FileRepository.RemoveRange(files));
        }

        public async Task MarkFilePermanentAsync(Guid fileId)
        {
            var file = await RunWithoutCommitAsync(uow => uow.FileRepository.GetAsync(f => f.IsTemporary && f.Id == fileId));
            if (file == null)
                throw new ArgumentException($"There is no temporary file with id {fileId}");
            file.IsTemporary = false;

            await RunAsync(uow => uow.FileRepository.Update(file));
        }

        private static Guid ComputeHash(byte[] data)
        {
            using HashAlgorithm algorithm = MD5.Create();
            byte[] bytes = algorithm.ComputeHash(data);
            return new Guid(bytes);
        }
    }
}
