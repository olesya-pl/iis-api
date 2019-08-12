using System;
using System.IO;
using System.Threading.Tasks;

namespace IIS.Core.Files
{
    public interface IFileService
    {
        Task<Guid> SaveFileAsync(Stream stream, string fileName, string contentType);
        Task<FileInfo> GetFileAsync(Guid id);
        Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate);
        Task MarkFilePermanentAsync(Guid fileId);
    }
}
