#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to get the albums of an artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist whose albums are retrieved.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistAlbumsRequest(
    Guid ArtistId
);
