#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object carrying the full details of a music artist, as stored in the media library.
/// </summary>
[DebuggerDisplay("Name: {Metadata?.Name}")]
public class MusicArtistDto
{
    /// <summary>
    /// Gets or sets the Id of the artist.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library this artist belongs to.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the path of the image representing the artist, if available.
    /// </summary>
    public string? CoverPath { get; set; }

    /// <summary>
    /// Gets or sets the metadata of the artist.
    /// </summary>
    public MusicArtistMetadataDto? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the website of the artist, if applicable.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the artist, if applicable.
    /// </summary>
    public Guid? MusicBrainzArtistId { get; set; }

    /// <summary>
    /// Gets or sets the list of IPI (Interested Party Information) codes of the artist.
    /// </summary>
    public List<string>? Ipis { get; set; }

    /// <summary>
    /// Gets or sets the list of ISNI (International Standard Name Identifier) codes of the artist.
    /// </summary>
    public List<string>? Isnis { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the artist was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional date and time when the artist was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the list of references to the media contributors that make up the artist, each with the role they played.
    /// </summary>
    public List<MediaContributorReferenceDto>? Contributors { get; set; }

    /// <summary>
    /// Gets or sets the list of ratings for this artist.
    /// </summary>
    public List<AudioRatingDto>? Ratings { get; set; }

    /// <summary>
    /// Gets or sets the list of albums of the artist, each with its own list of tracks.
    /// </summary>
    public List<MusicAlbumDto>? Albums { get; set; }
}
