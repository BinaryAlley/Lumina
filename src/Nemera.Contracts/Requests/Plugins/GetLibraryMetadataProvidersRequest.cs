#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to get the metadata providers configured for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose metadata providers are retrieved. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public sealed record GetLibraryMetadataProvidersRequest(
    Guid LibraryId
);
