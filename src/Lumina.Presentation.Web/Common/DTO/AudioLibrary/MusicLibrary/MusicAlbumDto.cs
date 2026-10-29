#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object carrying the full details of an album, as stored in the media library.
/// </summary>
[DebuggerDisplay("Title: {Metadata?.Title}")]
public class MusicAlbumDto
{
    /// <summary>
    /// Gets or sets the Id of the album.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Id of the artist the album belongs to.
    /// </summary>
    public Guid ArtistId { get; set; }

    /// <summary>
    /// Gets or sets the name of the artist the album belongs to.
    /// </summary>
    public string ArtistName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Id of the media library this album belongs to.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the path of the image representing the album, if available.
    /// </summary>
    public string? CoverPath { get; set; }

    /// <summary>
    /// Gets or sets the stored artwork of the album, in display order, so that all of its pieces can be presented, not just the cover.
    /// </summary>
    public List<MediaArtworkDto>? Artwork { get; set; }

    /// <summary>
    /// Gets or sets the album metadata of the album.
    /// </summary>
    public MusicAlbumMetadataDto? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the physical or digital medium of the album, if applicable.
    /// </summary>
    public MusicMediaFormat? MediaFormat { get; set; }

    /// <summary>
    /// Gets or sets the outermost physical packaging of the album, if applicable.
    /// </summary>
    public MusicReleasePackaging? Packaging { get; set; }

    /// <summary>
    /// Gets or sets the barcode of the album, if applicable.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Gets or sets the catalog numbers of the album, if applicable.
    /// </summary>
    public List<string>? CatalogNumbers { get; set; }

    /// <summary>
    /// Gets or sets the name of the label that issued the album, if applicable.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the ASIN (Amazon Standard Identification Number) of the album, if applicable.
    /// </summary>
    public string? ASIN { get; set; }

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
    /// Gets or sets the date and time when the album was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional date and time when the album was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the list of references to the media contributors that performed on the album, each with the role they played.
    /// </summary>
    public List<MediaContributorReferenceDto>? Contributors { get; set; }

    /// <summary>
    /// Gets or sets the list of ratings for this album.
    /// </summary>
    public List<AudioRatingDto>? Ratings { get; set; }

    /// <summary>
    /// Gets or sets the list of tracks of the album.
    /// </summary>
    public List<MusicTrackDto>? Tracks { get; set; }
}
