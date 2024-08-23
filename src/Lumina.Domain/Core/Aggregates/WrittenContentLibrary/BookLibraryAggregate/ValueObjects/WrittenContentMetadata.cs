#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the metadata of a written media element.
/// </summary>
[DebuggerDisplay("{Title}")]
public class WrittenContentMetadata : BaseMetadata
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the publisher of the written content.
    /// </summary>
    public Optional<string> Publisher { get; }

    /// <summary>
    /// Gets the number of pages in the written content.
    /// </summary>
    public Optional<int> PageCount { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="WrittenContentMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the written content.</param>
    /// <param name="originalTitle">The optional original title of the video.</param>
    /// <param name="release">The release information of the written content.</param>
    /// <param name="description">The description of the written content.</param>
    /// <param name="genres">The genres of the written content.</param>
    /// <param name="tags">The tags associated with the written content.</param>
    /// <param name="language">The language of the written content.</param>
    /// <param name="originalLanguage">The optional original language of the video.</param>
    /// <param name="publisher">The publisher of the written content.</param>
    /// <param name="pageCount">The number of pages in the written content.</param>
    public WrittenContentMetadata(
        string title,
        Optional<string> originalTitle,
        ReleaseInfo release,
        Optional<string> description,
        List<Genre> genres,
        List<Tag> tags,
        Optional<LanguageInfo> language, 
        Optional<LanguageInfo> originalLanguage,
        Optional<string> publisher,
        Optional<int> pageCount)
        : base(title, originalTitle, release, description, genres, tags, language, originalLanguage)
    {
        Publisher = publisher;
        PageCount = pageCount;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
            yield return component;
        yield return Publisher;
        yield return PageCount;
    }
    #endregion
}