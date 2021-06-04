using Iis.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using File = Iis.Domain.Materials.File;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IFileService
    {
        Task<FileIdDto> SaveFileAsync(Stream stream, string fileName, string contentType, CancellationToken token);
        Task<File> GetFileAsync(Guid id);
        Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate);
        Task MarkFilePermanentAsync(Guid fileId);
        Task<FileIdDto> IsDuplicatedAsync(Stream contents);
        int RemoveFiles(List<Guid> ids);
    }
}
