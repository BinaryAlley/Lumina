#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to report the progress of a media library scan.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose scan progress is requested. Required.</param>
/// <param name="ScanId">The Id of the media library scan whose progress is requested. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}, ScanId: {ScanId}")]
public record GetLibraryScanProgressRequest(
    Guid LibraryId,
    Guid ScanId
);
