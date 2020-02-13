using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileInfo = Iis.Domain.Materials.FileInfo;

namespace IIS.Core.Files
{
    public interface IFileService
    {
        Task<FileId> SaveFileAsync(Stream stream, string fileName, string contentType, CancellationToken token);
        Task<FileInfo> GetFileAsync(Guid id);
        Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate);
        Task MarkFilePermanentAsync(Guid fileId);
    }
}
