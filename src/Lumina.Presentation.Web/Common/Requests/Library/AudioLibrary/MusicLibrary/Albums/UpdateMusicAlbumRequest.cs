#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request to update an existing album of a music artist.
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
[DebuggerDisplay("Title: {Metadata?.Title}")]
public record UpdateMusicAlbumRequest(
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
    List<AudioRatingDto>? Ratings
);
