#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Security;

/// <summary>
/// Interface for the service for hashing files by sampling chunks from them.
/// </summary>
public interface IFileHashService
{
    /// <summary>
    /// Hashes <paramref name="files"/> by sampling chunks from them.
    /// </summary>
    /// <param name="files">The collection of files to hash.</param>
    /// <param name="callback">Callback to invoke during processing of elements.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A collection of the hashed files, along with their hashes.</returns>
    Task<List<HashedFileSystemFileDto>> HashFilesAsync(IReadOnlyCollection<HashedFileSystemFileDto> files, Func<Task> callback, CancellationToken cancellationToken);

    /// <summary>
    /// Computes the content hash of the file stored at <paramref name="filePath"/>, by sampling chunks from it.
    /// </summary>
    /// <param name="filePath">The path of the file to hash.</param>
    /// <returns>The content hash of the file.</returns>
    ulong ComputeFileHash(string filePath);
}
