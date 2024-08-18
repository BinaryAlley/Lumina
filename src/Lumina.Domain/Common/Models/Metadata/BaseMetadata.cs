#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Models.Metadata;

/// <summary>
/// Base class for all metadata-related value objects.
/// </summary>
public class BaseMetadata : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    public string Title { get; }
    public Optional<string> Description { get; }
    public Optional<DateTime> ReleaseDate { get; }
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyList<string> Genres { get; }
    public Optional<string> Language { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="title">The title of the element to which this metadata object belongs to.</param>
    /// <param name="releaseDate">The optional release date of the element to which this metadata object belongs to.</param>
    /// <param name="description">The optional description of the element to which this metadata object belongs to.</param>
    /// <param name="genres">The list of genres of the element to which this metadata object belongs to.</param>
    /// <param name="tags">The list of tags of the element to which this metadata object belongs to.</param>
    /// <param name="language">The optional language of the element to which this metadata object belongs to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public BaseMetadata(string title, Optional<DateTime> releaseDate, Optional<string> description, IEnumerable<string>? genres, IEnumerable<string>? tags, Optional<string> language)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        ReleaseDate = releaseDate;
        Description = description;
        Genres = genres?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        Tags = tags?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        Language = language;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title;
        yield return ReleaseDate;
        yield return Description;
        foreach (var genre in Genres)
            yield return genre;
        foreach (var tag in Tags)
            yield return tag;
        yield return Language;
    }
    #endregion
}