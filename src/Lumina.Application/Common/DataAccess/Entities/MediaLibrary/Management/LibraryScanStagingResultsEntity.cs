#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for a media library scan staging result, holding the file system items discovered during a scan, before they are compared against the media library scan snapshot of the previous scan.
/// </summary>
[DebuggerDisplay("Id: {Id}; Path: {Path}")]
public class LibraryScanStagingResultsEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the media library scan staging result.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the media library scan that this staging result belongs to.
    /// </summary>
    public required Guid LibraryScanId { get; init; }

    /// <summary>
    /// Gets the path of the media library scan staging result.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the size of the media library scan staging result, in bytes.
    /// </summary>
    public required long Size { get; init; }

    /// <summary>
    /// Gets the time and date when the file system item was last modified, stored in ticks.
    /// </summary>
    public required long Ticks { get; init; }

    /// <summary>
    /// Gets the hash calculated for the media library scan staging result.
    /// </summary>
    public required ulong ContentHash { get; init; }

    /// <summary>
    /// Gets the hash of the same file system item, as stored in the media library scan snapshot of a previous scan, or 0 if the file system item is new.
    /// </summary>
    public required ulong PreviousContentHash { get; init; }

    /// <summary>
    /// Gets whether this media library scan staging result represents a file system item that needs its content to be hashed, because it is either new, or it has changed since the previous scan.
    /// </summary>
    public required bool NeedsRehash { get; init; }

    /// <summary>
    /// Gets whether this media library scan staging result represents a file system item that was not present in the media library scan snapshot of a previous scan.
    /// </summary>
    public required bool IsNew { get; init; }
}
