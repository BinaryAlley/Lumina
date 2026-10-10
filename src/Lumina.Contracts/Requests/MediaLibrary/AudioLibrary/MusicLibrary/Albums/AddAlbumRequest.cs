#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request to add an album to an artist, together with the tracks of the album.
/// </summary>
/// <param name="Metadata">The album metadata of the album. Required.</param>
/// <param name="MediaFormat">The physical or digital medium of the album. Optional.</param>
/// <param name="Packaging">The outermost physical packaging of the album. Optional.</param>
/// <param name="Barcode">The barcode of the album. Optional.</param>
/// <param name="CatalogNumbers">The catalog numbers of the album. Optional.</param>
/// <param name="Label">The name of the label that issued the album. Optional.</param>
/// <param name="ASIN">The ASIN (Amazon Standard Identification Number) of the album. Optional.</param>
/// <param name="MusicBrainzReleaseId">The MusicBrainz identifier of the release. Optional.</param>
/// <param name="MusicBrainzReleaseGroupId">The MusicBrainz identifier of the release group. Optional.</param>
/// <param name="MusicBrainzReleaseArtistId">The MusicBrainz identifier of the release artist. Optional.</param>
/// <param name="Contributors">The list of media contributors that performed on the album. Required.</param>
/// <param name="Ratings">The list of ratings for this album. Required.</param>
/// <param name="Tracks">The list of tracks of the album. Required.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record AddAlbumRequest(
    MusicAlbumMetadataDto? Metadata,
    MusicMediaFormat? MediaFormat,
    MusicReleasePackaging? Packaging,
    string? Barcode,
    List<string>? CatalogNumbers,
    string? Label,
    string? ASIN,
    Guid? MusicBrainzReleaseId,
    Guid? MusicBrainzReleaseGroupId,
    Guid? MusicBrainzReleaseArtistId,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<AddTrackRequest>? Tracks
);
