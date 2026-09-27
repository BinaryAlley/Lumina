#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
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
    /// Gets or sets the website of the artist, if applicable.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the artist, if applicable.
    /// </summary>
    public Guid? MusicBrainzArtistId { get; set; }

    /// <summary>
    /// Gets or sets the albums of the artist.
    /// </summary>
    public List<AlbumEntity> Albums { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of the media contributors that make up the artist.
    /// </summary>
    public List<ArtistContributorEntity> Contributors { get; set; } = [];

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
