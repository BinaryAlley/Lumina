#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Models.Common;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Books;

/// <summary>
/// Represents written content metadata.
/// </summary>
public class WrittenContentMetadataDto
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the title of the media item.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets the original title of the media item.
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets the optional description of the media item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the release information of the media item.
    /// </summary>
    public ReleaseInfoDto? ReleaseInfo { get; set; }

    /// <summary>
    /// Gets the list of genres associated with the media item.
    /// </summary>
    public List<GenreDto>? Genres { get; set; }

    /// <summary>
    /// Gets the list of tags associated with the media item.
    /// </summary>
    public List<TagDto>? Tags { get; set; }

    /// <summary>
    /// Gets the optional language of the media item.
    /// </summary>
    public LanguageInfoDto? Language { get; set; }

    /// <summary>
    /// Gets the optional original language of the media item. 
    /// </summary>
    public LanguageInfoDto? OriginalLanguage { get; set; }

    /// <summary>
    /// Gets the publisher of the written content.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets the number of pages in the written content.
    /// </summary>
    public int? PageCount { get; set; }
    #endregion
}