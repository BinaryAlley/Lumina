#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to get the artwork providers configured for a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose artwork providers are retrieved. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public sealed record GetLibraryArtworkProvidersRequest(
    Guid LibraryId
);
