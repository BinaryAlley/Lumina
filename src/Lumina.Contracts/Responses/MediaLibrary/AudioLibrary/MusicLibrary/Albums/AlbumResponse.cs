#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents an album response.
/// </summary>
/// <param name="Id">The Id of the album.</param>
/// <param name="ArtistId">The Id of the artist the album belongs to.</param>
/// <param name="LibraryId">The Id of the media library this album belongs to.</param>
/// <param name="Metadata">The album metadata of the album.</param>
/// <param name="MediaFormat">The physical or digital medium of the album, if applicable.</param>
/// <param name="Packaging">The outermost physical packaging of the album, if applicable.</param>
/// <param name="Barcode">The barcode of the album, if applicable.</param>
/// <param name="CatalogNumbers">The catalog numbers of the album, if applicable.</param>
/// <param name="Label">The name of the label that issued the album, if applicable.</param>
/// <param name="ASIN">The ASIN (Amazon Standard Identification Number) of the album, if applicable.</param>
/// <param name="MusicBrainzReleaseId">The MusicBrainz identifier of the release, if applicable.</param>
/// <param name="MusicBrainzReleaseGroupId">The MusicBrainz identifier of the release group, if applicable.</param>
/// <param name="MusicBrainzReleaseArtistId">The MusicBrainz identifier of the release artist, if applicable.</param>
/// <param name="CreatedOnUtc">The date and time when the album was created.</param>
/// <param name="UpdatedOnUtc">The optional date and time when the album was updated.</param>
/// <param name="Contributors">The list of references to the media contributors that performed on the album, each with the role they played.</param>
/// <param name="Ratings">The list of ratings for this album.</param>
/// <param name="Tracks">The list of tracks of the album.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record AlbumResponse(
    Guid Id,
    Guid ArtistId,
    Guid LibraryId,
    MusicAlbumMetadataDto Metadata,
    MusicMediaFormat? MediaFormat,
    MusicReleasePackaging? Packaging,
    string? Barcode,
    List<string>? CatalogNumbers,
    string? Label,
    string? ASIN,
    Guid? MusicBrainzReleaseId,
    Guid? MusicBrainzReleaseGroupId,
    Guid? MusicBrainzReleaseArtistId,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<TrackResponse>? Tracks
);
