#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to add an artist, together with the albums of the artist and the tracks of those albums.
/// </summary>
/// <param name="Metadata">The metadata of the artist. Required.</param>
/// <param name="Website">The website of the artist. Optional.</param>
/// <param name="MusicBrainzArtistId">The MusicBrainz identifier of the artist. Optional.</param>
/// <param name="Ipis">The list of IPI (Interested Party Information) codes of the artist. Optional.</param>
/// <param name="Isnis">The list of ISNI (International Standard Name Identifier) codes of the artist. Optional.</param>
/// <param name="Contributors">The list of media contributors that make up the artist. Required.</param>
/// <param name="Ratings">The list of ratings for the artist. Required.</param>
/// <param name="Albums">The list of albums of the artist, each with its own list of tracks. Required.</param>
[DebuggerDisplay("Name: {Metadata.Name}")]
public record AddArtistRequest(
    MusicArtistMetadataDto? Metadata,
    string? Website,
    Guid? MusicBrainzArtistId,
    List<string>? Ipis,
    List<string>? Isnis,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<AddAlbumRequest>? Albums
);
