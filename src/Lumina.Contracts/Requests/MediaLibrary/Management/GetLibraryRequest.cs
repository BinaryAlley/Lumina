#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to get a media library by its Id.
/// </summary>
/// <param name="Id">The unique identifier of the media library. Required.</param>
[DebuggerDisplay("Id: {Id}")]
public record GetLibraryRequest(
    Guid Id
);
