#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents an artist response.
/// </summary>
/// <param name="Id">The Id of the artist.</param>
/// <param name="LibraryId">The Id of the media library this artist belongs to.</param>
/// <param name="Metadata">The metadata of the artist.</param>
/// <param name="Website">The website of the artist, if applicable.</param>
/// <param name="MusicBrainzArtistId">The MusicBrainz identifier of the artist, if applicable.</param>
/// <param name="Ipis">The list of IPI (Interested Party Information) codes of the artist.</param>
/// <param name="Isnis">The list of ISNI (International Standard Name Identifier) codes of the artist.</param>
/// <param name="CreatedOnUtc">The date and time when the artist was created.</param>
/// <param name="UpdatedOnUtc">The optional date and time when the artist was updated.</param>
/// <param name="Contributors">The list of references to the media contributors that make up the artist, each with the role they played.</param>
/// <param name="Ratings">The list of ratings for this artist.</param>
/// <param name="Albums">The list of albums of the artist, each with its own list of tracks.</param>
[DebuggerDisplay("Name: {Metadata.Name}")]
public record ArtistResponse(
    Guid Id,
    Guid LibraryId,
    MusicArtistMetadataDto Metadata,
    string? Website,
    Guid? MusicBrainzArtistId,
    List<string>? Ipis,
    List<string>? Isnis,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<AlbumResponse>? Albums
);
