using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Files.EntityFramework
{
    public class FileService : IFileService
    {
        private OntologyContext _context;

        public FileService(OntologyContext context)
        {
            _context = context;
        }

        public async Task<Guid> SaveFileAsync(Stream stream, string fileName, string contentType)
        {
            byte[] contents;
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                contents = ms.ToArray();
            }

            var file = new File
            {
                Name = fileName,
                ContentType = contentType,
                Contents = contents,
                UploadTime = DateTime.Now, // todo: move to db or to datetime provider
                IsTemporary = true,
            };
            _context.Add(file);
            await _context.SaveChangesAsync();
            return file.Id;
        }

        public async Task<FileInfo> GetFileAsync(Guid id)
        {
            await _context.Semaphore.WaitAsync();
            File file;
            try
            {
                file = _context.Files.SingleOrDefault(f => f.Id == id);
                if (file == null) return null;
            }
            finally
            {
                _context.Semaphore.Release();
            }
            var ms = new MemoryStream(file.Contents);
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
    }
}
