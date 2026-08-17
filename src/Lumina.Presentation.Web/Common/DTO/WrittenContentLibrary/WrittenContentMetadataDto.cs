#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary;

/// <summary>
/// Data transfer object for written content metadata.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class WrittenContentMetadataDto
{
    /// <summary>
    /// Gets or sets the title of the written content.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the original title of the written content, if different from the current title.
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets a brief description or summary of the written content.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the release information, including release date and other relevant details.
    /// </summary>
    public ReleaseInfoDto? ReleaseInfo { get; set; }

    /// <summary>
    /// Gets or sets the list of genres associated with the written content.
    /// </summary>
    public List<GenreDto>? Genres { get; set; }

    /// <summary>
    /// Gets or sets the list of tags that further describe or categorize the written content.
    /// </summary>
    public List<TagDto>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the language in which the written content is written.
    /// </summary>
    public LanguageInfoDto? Language { get; set; }

    /// <summary>
    /// Gets or sets the original language of the written content, if it has been translated.
    /// </summary>
    public LanguageInfoDto? OriginalLanguage { get; set; }

    /// <summary>
    /// Gets or sets the name of the publisher of the written content.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the number of pages in the written content.
    /// </summary>
    public int? PageCount { get; set; }
}
