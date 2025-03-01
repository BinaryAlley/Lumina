#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for a media library scan result involving a file.
/// </summary>
[DebuggerDisplay("Id: {Id}; Status: {Status}")]
public class LibraryScanResultEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the media library scan result.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the scan that this scan result belongs to.
    /// </summary>
    public required Guid LibraryScanId { get; init; }

    /// <summary>
    /// Gets the path of the media library scan file.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the hash calculated for the media library scan file.
    /// </summary>
    public required ulong ContentHash { get; init; }

    /// <summary>
    /// Gets the size of the media library scan file.
    /// </summary>
    public required long FileSize { get; init; }

    /// <summary>
    /// Gets the modification date of the media library scan file.
    /// </summary>
    public required DateTime LastModified { get; init; }

    /// <summary>
    /// Gets the media library scan that this scan result belongs to.
    /// </summary>
    public required LibraryScanEntity LibraryScan { get; init; } = null!;
}
