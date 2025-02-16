#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to cancel the scan of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose scan is cancelled. Required.</param>
/// <param name="ScanId">The Id of scan to cancel. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}; ScanId: {ScanId}")]
public record CancelLibraryScanRequest(
    Guid LibraryId,
    Guid ScanId
);
