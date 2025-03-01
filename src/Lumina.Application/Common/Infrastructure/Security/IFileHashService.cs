#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Infrastructure.Models.MediaLibraryScanJobPayloads;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// <param name="previousScanResults">Lookup dictionary of a previous media library scan, used to determine if a file changed or not.</param>
    /// <param name="callback">Callback to invoke during processing of elements.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A collection of files that changed since last library scan, along with their hashes.</returns>
    List<ChangedFileSystemFile> HashFiles(
        List<FileInfo> files,
        Dictionary<string, LibraryScanResultEntity> previousScanResults,
        Func<Task> callback,
        CancellationToken cancellationToken);
}
