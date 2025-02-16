#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to delete a media library by its Id.
/// </summary>
/// <param name="Id">The unique identifier of the media library to be deleted. Required.</param>
[DebuggerDisplay("Id: {Id}")]
public record DeleteLibraryRequest(
    Guid Id
);
