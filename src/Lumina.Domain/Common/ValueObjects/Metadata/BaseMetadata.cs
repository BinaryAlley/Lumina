#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Base template for all metadata-related value objects.
/// </summary>
public abstract class BaseMetadata : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the title of the media item.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the original title of the media item.
    /// </summary>
    public Optional<string> OriginalTitle { get; }

    /// <summary>
    /// Gets the optional description of the media item.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// Gets the release information of the media item.
    /// </summary>
    public ReleaseInfo ReleaseInfo { get; }

    /// <summary>
    /// Gets the list of tags associated with the media item.
    /// </summary>
    public IReadOnlyList<Tag> Tags { get; }
    
    /// <summary>
    /// Gets the list of genres associated with the media item.
    /// </summary>
    public IReadOnlyList<Genre> Genres { get; }
    
    /// <summary>
    /// Gets the optional language of the media item.
    /// </summary>
    public Optional<LanguageInfo> Language { get; }
    
    /// <summary>
    /// Gets the optional original language of the media item. 
    /// </summary>
    public Optional<LanguageInfo> OriginalLanguage { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="title">The title of the element to which this metadata object belongs to.</param>
    /// <param name="originalTitle">The optional original title of the element to which this metadata object belongs to.</param>
    /// <param name="releaseInfo">The optional release information of the media item.</param>
    /// <param name="description">The optional description of the element to which this metadata object belongs to.</param>
    /// <param name="genres">The list of genres of the element to which this metadata object belongs to.</param>
    /// <param name="tags">The list of tags of the element to which this metadata object belongs to.</param>
    /// <param name="language">The optional language of the element to which this metadata object belongs to.</param>
    /// <param name="originalLanguage">The optional original language of the element to which this metadata object belongs to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <see cref="title"/> value is <see langword="null"/></exception>
    protected BaseMetadata(string title, Optional<string> originalTitle, ReleaseInfo releaseInfo, Optional<string> description, 
        IEnumerable<Genre>? genres, IEnumerable<Tag>? tags, Optional<LanguageInfo> language, Optional<LanguageInfo> originalLanguage)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        OriginalTitle = originalTitle;
        ReleaseInfo = releaseInfo;
        Description = description;
        Genres = genres?.ToList().AsReadOnly() ?? new List<Genre>().AsReadOnly();
        Tags = tags?.ToList().AsReadOnly() ?? new List<Tag>().AsReadOnly();
        Language = language;
        OriginalLanguage = originalLanguage;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title;
        yield return OriginalTitle;
        yield return ReleaseInfo;
        yield return Description;
        foreach (var genre in Genres)
            yield return genre;
        foreach (var tag in Tags)
            yield return tag;
        yield return Language;
        yield return OriginalLanguage;
    }
    #endregion
}