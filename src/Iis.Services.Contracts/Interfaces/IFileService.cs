using Iis.Services.Contracts.Dtos;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IFileService
    {
        Task<FileIdDto> SaveFileAsync(Stream stream, string fileName, string contentType, CancellationToken token);
        Task<FileDto> GetFileAsync(Guid id);
        Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate);
        Task MarkFilePermanentAsync(Guid fileId);
    }
}
