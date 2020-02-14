using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Iis.Api.Configuration;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FileInfo = Iis.Domain.Materials.FileInfo;

namespace IIS.Core.Files.EntityFramework
{
    public class FileService : IFileService
    {
        private OntologyContext _context;
        private readonly FilesConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        public FileService(OntologyContext context, FilesConfiguration configuration, ILogger<FileService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FileId> SaveFileAsync(Stream stream, string fileName, string contentType, CancellationToken token)
        {
            byte[] contents;
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms, token);
                contents = ms.ToArray();
            }

            Guid hash = ComputeHash(contents);

            // Check for duplicates
            var query =
                from f in _context.Files
                where f.ContentHash == hash
                select new
                {
                    f.Id,
                    f.Contents
                };
            var files = await query.ToArrayAsync(token);
            foreach (var fileData in files)
            {
                if (fileData.Contents.Length != contents.Length)
                {
                    continue;
                }

                if (contents.SequenceEqual(fileData.Contents))
                {
                    // Duplicate found.
                    return new FileId
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

            _context.Add(file);
            await _context.SaveChangesAsync();

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

            return new FileId
            {
                Id = file.Id
            };
        }

        public async Task<FileInfo> GetFileAsync(Guid id)
        {
            await _context.Semaphore.WaitAsync();
            FileEntity file;
            try
            {
                file = _context.Files.SingleOrDefault(f => f.Id == id);
                if (file == null) return null;
            }
            finally
            {
                _context.Semaphore.Release();
            }

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

            var ms = new MemoryStream(contents);
            return new FileInfo(id, file.Name, file.ContentType, ms, file.IsTemporary);
        }

        public async Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate)
        {
            var files = _context.Files.Where(f => f.IsTemporary && predicate(f.UploadTime));
            _context.Files.RemoveRange(files);
            await _context.SaveChangesAsync();
        }

        public async Task MarkFilePermanentAsync(Guid fileId)
        {
            var file = await _context.Files.SingleOrDefaultAsync(f => f.IsTemporary && f.Id == fileId);
            if (file == null)
                throw new ArgumentException($"There is no temporary file with id {fileId}");
            file.IsTemporary = false;
            await _context.SaveChangesAsync();
        }

        private static Guid ComputeHash(byte[] data)
        {
            using HashAlgorithm algorithm = MD5.Create();
            byte[] bytes = algorithm.ComputeHash(data);
            return new Guid(bytes);
        }
    }
}
