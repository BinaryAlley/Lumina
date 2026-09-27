#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to add an artist, together with the albums of the artist and the tracks of those albums.
/// </summary>
/// <param name="Name">The name of the artist. Required.</param>
/// <param name="Website">The website of the artist. Optional.</param>
/// <param name="MusicBrainzArtistId">The MusicBrainz identifier of the artist. Optional.</param>
/// <param name="Contributors">The list of media contributors that make up the artist. Required.</param>
/// <param name="Albums">The list of albums of the artist, each with its own list of tracks. Required.</param>
[DebuggerDisplay("Name: {Name}")]
public record AddArtistRequest(
    string? Name,
    string? Website,
    Guid? MusicBrainzArtistId,
    List<MediaContributorReferenceDto>? Contributors,
    List<AddAlbumRequest>? Albums
);
