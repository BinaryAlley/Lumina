#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a media library scan response.
/// </summary>
/// <param name="ScanId">The Id of the media library scan.</param>
/// <param name="LibraryId">The Id of the scanned media library.</param>
[DebuggerDisplay("ScanId: {ScanId} LibraryId: {LibraryId}")]
public record ScanLibraryResponse(
    Guid ScanId,
    Guid LibraryId  
);
