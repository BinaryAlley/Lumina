#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for a directory scan fingerprint, holding the last write time of a directory of a media library, used to skip the scanning of directories that have not changed since the last scan.
/// </summary>
[DebuggerDisplay("Id: {Id}; Path: {Path}")]
public class DirectoryScanFingerprintEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the directory scan fingerprint.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the media library to which this directory scan fingerprint belongs.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// Gets the path of the directory to which this directory scan fingerprint belongs.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the last write time of the directory, in UTC, at the moment when the directory was scanned.
    /// </summary>
    public required DateTime LastWriteTimeUtc { get; init; }
}
