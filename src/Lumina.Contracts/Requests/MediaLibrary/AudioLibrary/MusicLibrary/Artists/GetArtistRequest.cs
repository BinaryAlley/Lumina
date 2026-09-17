#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to get an artist by the specified Id.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist to retrieve.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistRequest(
    Guid ArtistId
);
