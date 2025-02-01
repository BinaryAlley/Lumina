#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to cancel the scan of a media library by its Id.
/// </summary>
/// <param name="Id">The Id of the media library whose scan is cancelled. Required.</param>
[DebuggerDisplay("Id: {Id}")]
public record CancelLibraryScanRequest(
    Guid Id
);
