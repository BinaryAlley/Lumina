#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for an album, a release of an artist.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class AlbumEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the album.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the artist the album belongs to.
    /// </summary>
    public required Guid ArtistId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library the artist of the album belongs to.
    /// </summary>
    public required Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the title of the album.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the original title of the album, if applicable.
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets the description of the album, if applicable.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the original release date of the album, if applicable.
    /// </summary>
    public DateOnly? OriginalReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the original release year of the album, if applicable.
    /// </summary>
    public int? OriginalReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the re-release or reissue date of the album, if applicable.
    /// </summary>
    public DateOnly? ReReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the re-release or reissue year of the album, if applicable.
    /// </summary>
    public int? ReReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the country or region of release, if applicable.
    /// </summary>
    public ReleaseCountry? ReleaseCountry { get; set; }

    /// <summary>
    /// Gets or sets the release version or edition of the album, if applicable.
    /// </summary>
    public string? ReleaseVersion { get; set; }

    /// <summary>
    /// Gets or sets the ISO 639-1 two-letter language code of the album, if applicable.
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the full name of the language of the album in English, if applicable.
    /// </summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// Gets or sets the native name of the language of the album, if applicable.
    /// </summary>
    public string? LanguageNativeName { get; set; }

    /// <summary>
    /// Gets or sets the ISO 639-1 two-letter original language code of the album, if applicable.
    /// </summary>
    public string? OriginalLanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the full name of the original language of the album in English, if applicable.
    /// </summary>
    public string? OriginalLanguageName { get; set; }

    /// <summary>
    /// Gets or sets the native name of the original language of the album, if applicable.
    /// </summary>
    public string? OriginalLanguageNativeName { get; set; }

    /// <summary>
    /// Gets or sets the type of the release, if applicable.
    /// </summary>
    public MusicReleaseType? ReleaseType { get; set; }

    /// <summary>
    /// Gets or sets the status of the release, if applicable.
    /// </summary>
    public MusicReleaseStatus? ReleaseStatus { get; set; }

    /// <summary>
    /// Gets or sets the number of discs of the release, if applicable.
    /// </summary>
    public int? TotalDiscs { get; set; }

    /// <summary>
    /// Gets or sets the number of tracks of the release.
    /// </summary>
    public int TotalTracks { get; set; }

    /// <summary>
    /// Gets or sets the physical or digital medium of the album, if applicable.
    /// </summary>
    public MusicMediaFormat? MediaFormat { get; set; }

    /// <summary>
    /// Gets or sets the barcode of the album, if applicable.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Gets or sets the catalog number of the album, if applicable.
    /// </summary>
    public string? CatalogNumber { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the release, if applicable.
    /// </summary>
    public Guid? MusicBrainzReleaseId { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the release group, if applicable.
    /// </summary>
    public Guid? MusicBrainzReleaseGroupId { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the release artist, if applicable.
    /// </summary>
    public Guid? MusicBrainzReleaseArtistId { get; set; }

    /// <summary>
    /// Gets or sets the artist the album belongs to.
    /// </summary>
    public ArtistEntity? Artist { get; set; }

    /// <summary>
    /// Gets or sets the tracks of the album.
    /// </summary>
    public List<TrackEntity> Tracks { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of ratings for this album.
    /// </summary>
    public List<AudioRatingEntity> Ratings { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of the media contributors of the album.
    /// </summary>
    public List<AlbumContributorEntity> Contributors { get; set; } = [];

    /// <summary>
    /// Gets or sets the genres of the album.
    /// </summary>
    public HashSet<GenreEntity> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags of the album.
    /// </summary>
    public HashSet<TagEntity> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public required DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public required Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public required Guid? UpdatedBy { get; set; }
}
