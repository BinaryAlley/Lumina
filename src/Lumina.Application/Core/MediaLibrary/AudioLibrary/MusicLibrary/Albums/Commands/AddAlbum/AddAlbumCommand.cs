#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;

/// <summary>
/// Command for adding an album to an artist.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this album belongs to, taken from the route.</param>
/// <param name="ArtistId">The Id of the artist the album belongs to, taken from the route.</param>
/// <param name="Metadata">The album metadata of the album.</param>
/// <param name="MediaFormat">The physical or digital medium of the album, if applicable.</param>
/// <param name="Barcode">The barcode of the album, if applicable.</param>
/// <param name="CatalogNumber">The catalog number of the album, if applicable.</param>
/// <param name="MusicBrainzReleaseId">The MusicBrainz identifier of the release, if applicable.</param>
/// <param name="MusicBrainzReleaseGroupId">The MusicBrainz identifier of the release group, if applicable.</param>
/// <param name="MusicBrainzReleaseArtistId">The MusicBrainz identifier of the release artist, if applicable.</param>
/// <param name="Contributors">The list of media contributors that performed on the album.</param>
/// <param name="Ratings">The list of ratings for this album.</param>
/// <param name="Tracks">The list of tracks of the album.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record AddAlbumCommand(
    string? LibraryId,
    string? ArtistId,
    AlbumMetadataDto? Metadata,
    MusicMediaFormat? MediaFormat,
    string? Barcode,
    string? CatalogNumber,
    Guid? MusicBrainzReleaseId,
    Guid? MusicBrainzReleaseGroupId,
    Guid? MusicBrainzReleaseArtistId,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<AddTrackCommand>? Tracks,
    Guid? AlbumId = null
) : ICommand;
