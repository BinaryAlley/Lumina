#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;

/// <summary>
/// Command for updating an album.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this album belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist this album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album, taken from the route.</param>
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
/// <param name="Contributors">The list of media contributors that performed on the album.</param>
/// <param name="Ratings">The list of ratings for this album.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record UpdateAlbumCommand(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
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
) : ICommand;
