#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to update an album of an artist, together with the tracks of the album, within an artist update.
/// </summary>
/// <param name="AlbumId">The Id of the album, when the album already exists. Optional.</param>
/// <param name="Metadata">The album metadata of the album. Required.</param>
/// <param name="MediaFormat">The physical or digital medium of the album. Optional.</param>
/// <param name="Barcode">The barcode of the album. Optional.</param>
/// <param name="CatalogNumber">The catalog number of the album. Optional.</param>
/// <param name="MusicBrainzReleaseId">The MusicBrainz identifier of the release. Optional.</param>
/// <param name="MusicBrainzReleaseGroupId">The MusicBrainz identifier of the release group. Optional.</param>
/// <param name="MusicBrainzReleaseArtistId">The MusicBrainz identifier of the release artist. Optional.</param>
/// <param name="Contributors">The list of media contributors that performed on the album. Required.</param>
/// <param name="Ratings">The list of ratings for this album. Required.</param>
/// <param name="Tracks">The list of tracks of the album, each with the Id of the track when the track already exists. Required.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record UpdateArtistAlbumRequest(
    Guid? AlbumId,
    AlbumMetadataDto? Metadata,
    MusicMediaFormat? MediaFormat,
    string? Barcode,
    string? CatalogNumber,
    Guid? MusicBrainzReleaseId,
    Guid? MusicBrainzReleaseGroupId,
    Guid? MusicBrainzReleaseArtistId,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<UpdateArtistTrackRequest>? Tracks
);
