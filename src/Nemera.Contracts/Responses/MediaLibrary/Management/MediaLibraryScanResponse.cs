#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a media library scan response.
/// </summary>
/// <param name="ScanId">The unique identifier of the media library scan.</param>
/// <param name="LibraryId">The unique identifier of the scanned media library.</param>
[DebuggerDisplay("ScanId: {ScanId} LibraryId: {LibraryId}")]
public record MediaLibraryScanResponse(
    Guid ScanId,
    Guid LibraryId  
);
