#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for an artist.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class ArtistEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the artist.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the media library this artist belongs to.
    /// </summary>
    public required Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the name of the artist.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the sort name of the artist, a variant of the name used when sorting artists, if applicable.
    /// </summary>
    public string? SortName { get; set; }

    /// <summary>
    /// Gets or sets the disambiguation comment of the artist, used to distinguish artists with the same name, if applicable.
    /// </summary>
    public string? Disambiguation { get; set; }

    /// <summary>
    /// Gets or sets the type of the artist, if applicable.
    /// </summary>
    public MusicArtistType? Type { get; set; }

    /// <summary>
    /// Gets or sets the gender of the artist, applicable only to artists that are persons or characters, if applicable.
    /// </summary>
    public MusicArtistGender? Gender { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 code of the country the artist is associated with, if applicable.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the date the artist started existing, if applicable.
    /// </summary>
    public DateOnly? LifeSpanBegin { get; set; }

    /// <summary>
    /// Gets or sets the date the artist stopped existing, if applicable.
    /// </summary>
    public DateOnly? LifeSpanEnd { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the artist no longer exists.
    /// </summary>
    public bool IsEnded { get; set; }

    /// <summary>
    /// Gets or sets the website of the artist, if applicable.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the artist, if applicable.
    /// </summary>
    public Guid? MusicBrainzArtistId { get; set; }

    /// <summary>
    /// Gets or sets the area the artist is primarily identified with, if applicable.
    /// </summary>
    public MusicAreaEntity? Area { get; set; }

    /// <summary>
    /// Gets or sets the area the artist began in, if applicable.
    /// </summary>
    public MusicAreaEntity? BeginArea { get; set; }

    /// <summary>
    /// Gets or sets the area the artist ended in, if applicable.
    /// </summary>
    public MusicAreaEntity? EndArea { get; set; }

    /// <summary>
    /// Gets or sets the list of alternative names of the artist.
    /// </summary>
    public List<ArtistAliasEntity> Aliases { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of IPI (Interested Party Information) codes of the artist.
    /// </summary>
    public List<ArtistIpiEntity> Ipis { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of ISNI (International Standard Name Identifier) codes of the artist.
    /// </summary>
    public List<ArtistIsniEntity> Isnis { get; set; } = [];

    /// <summary>
    /// Gets or sets the genres of the artist.
    /// </summary>
    public HashSet<GenreEntity> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags of the artist.
    /// </summary>
    public HashSet<TagEntity> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of ratings of the artist.
    /// </summary>
    public List<AudioRatingEntity> Ratings { get; set; } = [];

    /// <summary>
    /// Gets or sets the albums of the artist.
    /// </summary>
    public List<AlbumEntity> Albums { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of the media contributors that make up the artist.
    /// </summary>
    public List<ArtistContributorEntity> Contributors { get; set; } = [];

    /// <summary>
    /// Gets or sets the status of the metadata enrichment of the artist.
    /// </summary>
    public MetadataStatus MetadataStatus { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the metadata of the artist was last enriched.
    /// </summary>
    public DateTime? LastMetadataUpdateUtc { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin that enriched the metadata of the artist.
    /// </summary>
    public string? MetadataProvider { get; set; }

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
