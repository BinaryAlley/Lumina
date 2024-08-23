#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Base template for all metadata-related value objects.
/// </summary>
[DebuggerDisplay("{Title}")]
public abstract class BaseMetadata : ValueObject
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly List<Tag> _tags;
    private readonly List<Genre> _genres;
    #endregion
    
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the title of the media item.
    /// </summary>
    public string Title { get; private set;}

    /// <summary>
    /// Gets the original title of the media item.
    /// </summary>
    public Optional<string> OriginalTitle { get; private set; }

    /// <summary>
    /// Gets the optional description of the media item.
    /// </summary>
    public Optional<string> Description { get; private set; }

    /// <summary>
    /// Gets the release information of the media item.
    /// </summary>
    public ReleaseInfo ReleaseInfo { get; private set; }

    /// <summary>
    /// Gets the optional language of the media item.
    /// </summary>
    public Optional<LanguageInfo> Language { get; private set; }
    
    /// <summary>
    /// Gets the optional original language of the media item. 
    /// </summary>
    public Optional<LanguageInfo> OriginalLanguage { get; private set; }

    /// <summary>
    /// Gets the list of tags associated with the media item.
    /// </summary>
    public IReadOnlyCollection<Tag> Tags 
    {
        get { return _tags.AsReadOnly(); }
    }
    
    /// <summary>
    /// Gets the list of genres associated with the media item.
    /// </summary>
    public IReadOnlyCollection<Genre> Genres 
    {
        get { return _genres.AsReadOnly(); }
    }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseMetadata"/> class.
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
    protected BaseMetadata(
        string title, 
        Optional<string> originalTitle, 
        ReleaseInfo releaseInfo, 
        Optional<string> description, 
        List<Genre> genres, 
        List<Tag> tags, 
        Optional<LanguageInfo> language, 
        Optional<LanguageInfo> originalLanguage)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        OriginalTitle = originalTitle;
        ReleaseInfo = releaseInfo;
        Description = description;
        _genres = genres;
        _tags = tags;
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
        foreach (var genre in _genres)
            yield return genre;
        foreach (var tag in _tags)
            yield return tag;
        yield return Language;
        yield return OriginalLanguage;
    }
    #endregion
}