using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;

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
            var file = _context.Files.SingleOrDefault(f => f.Id == id);
            if (file == null) return null;
            var ms = new MemoryStream(file.Contents);
            return new FileInfo(id, file.Name, file.ContentType, ms);
        }

        public async Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate)
        {
            var files = _context.Files.Where(f => f.IsTemporary && predicate(f.UploadTime));
            _context.Files.RemoveRange(files);
            await _context.SaveChangesAsync();
        }

        public async Task MarkFilePermanentAsync(Guid fileId)
        {
            var file = _context.Files.Single(f => f.IsTemporary && f.Id == fileId);
            file.IsTemporary = false;
            await _context.SaveChangesAsync();
        }
    }
}
