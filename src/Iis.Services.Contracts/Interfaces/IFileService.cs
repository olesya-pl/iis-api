using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Iis.Services.Contracts.Dtos;
using DomainMaterials = Iis.Domain.Materials;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IFileService
    {
        Task<FileResult> SaveFileAsync(Stream stream, string fileName, string contentType, CancellationToken token);
        Task<DomainMaterials.File> GetFileAsync(Guid id);
        Task<FileContentResult> GetFileContentAsync(Guid id);
        Task FlushTemporaryFilesAsync(Predicate<DateTime> predicate);
        Task MarkFilePermanentAsync(Guid fileId);
        Task<FileResult> IsDuplicatedAsync(Stream contents);
        int RemoveFiles(List<Guid> ids);
    }
}
