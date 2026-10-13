#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the metadata of an album.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class MusicAlbumMetadataDto
{
    /// <summary>
    /// Gets or sets the title of the album.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the original title of the album, if different from the current title.
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets a brief description or summary of the album.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the disambiguation comment of the album, used to distinguish albums with the same title.
    /// </summary>
    public string? Disambiguation { get; set; }

    /// <summary>
    /// Gets or sets the release information, including release date and other relevant details.
    /// </summary>
    public ReleaseInfoDto? ReleaseInfo { get; set; }

    /// <summary>
    /// Gets or sets the language of the album.
    /// </summary>
    public LanguageInfoDto? Language { get; set; }

    /// <summary>
    /// Gets or sets the original language of the album, if it has been translated.
    /// </summary>
    public LanguageInfoDto? OriginalLanguage { get; set; }

    /// <summary>
    /// Gets or sets the list of tags that further describe or categorize the album.
    /// </summary>
    public List<TagDto>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the list of genres associated with the album.
    /// </summary>
    public List<GenreDto>? Genres { get; set; }

    /// <summary>
    /// Gets or sets the script used by the language of the release of the album.
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Gets or sets the set of types of the release.
    /// </summary>
    public List<MusicReleaseType>? ReleaseTypes { get; set; }

    /// <summary>
    /// Gets or sets the status of the release.
    /// </summary>
    public MusicReleaseStatus? ReleaseStatus { get; set; }

    /// <summary>
    /// Gets or sets the number of discs of the release.
    /// </summary>
    public int? TotalDiscs { get; set; }

    /// <summary>
    /// Gets or sets the number of tracks of the release.
    /// </summary>
    public int? TotalTracks { get; set; }

    /// <summary>
    /// Gets or sets the title of the specific release (edition) the album files match, if different from the release group title.
    /// </summary>
    public string? ReleaseTitle { get; set; }
}
