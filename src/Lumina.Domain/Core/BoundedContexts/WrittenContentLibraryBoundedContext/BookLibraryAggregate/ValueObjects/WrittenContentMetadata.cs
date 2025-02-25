#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the metadata of a written media element.
/// </summary>
[DebuggerDisplay("{Title}")]
public class WrittenContentMetadata : BaseMetadata
{
    /// <summary>
    /// Gets the publisher of the written content, if applicable.
    /// </summary>
    public Optional<string> Publisher { get; }

    /// <summary>
    /// Gets the number of pages in the written content, if applicable.
    /// </summary>
    public Optional<int> PageCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WrittenContentMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the written content.</param>
    /// <param name="originalTitle">The optional original title of the written content, if applicable.</param>
    /// <param name="description">The description of the written content, if applicable.</param>
    /// <param name="releaseInfo">The release information of the written content.</param>
    /// <param name="genres">The genres of the written content.</param>
    /// <param name="tags">The tags associated with the written content.</param>
    /// <param name="language">The language of the written content, if applicable.</param>
    /// <param name="originalLanguage">The optional original language of the written content, if applicable.</param>
    /// <param name="publisher">The publisher of the written content, if applicable.</param>
    /// <param name="pageCount">The number of pages in the written content, if applicable.</param>
    private WrittenContentMetadata(
        string title,
        Optional<string> originalTitle,
        Optional<string> description,
        ReleaseInfo releaseInfo,
        List<Genre> genres,
        List<Tag> tags,
        Optional<LanguageInfo> language,
        Optional<LanguageInfo> originalLanguage,
        Optional<string> publisher,
        Optional<int> pageCount)
        : base(title, originalTitle, description, releaseInfo, genres, tags, language, originalLanguage)
    {
        Publisher = publisher;
        PageCount = pageCount;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="WrittenContentMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the written content.</param>
    /// <param name="originalTitle">The optional original title of the written content, if applicable.</param>
    /// <param name="description">The description of the written content, if applicable.</param>
    /// <param name="releaseInfo">The release information of the written content.</param>
    /// <param name="genres">The genres of the written content.</param>
    /// <param name="tags">The tags associated with the written content.</param>
    /// <param name="language">The language of the written content.</param>
    /// <param name="originalLanguage">The optional original language of the written content, if applicable.</param>
    /// <param name="publisher">The publisher of the written content, if applicable.</param>
    /// <param name="pageCount">The number of pages in the written content, if applicable.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="WrittenContentMetadata"/>, or an error message.
    /// </returns>
    public static ErrorOr<WrittenContentMetadata> Create(
        string title,
        Optional<string> originalTitle,
        Optional<string> description,
        ReleaseInfo releaseInfo,
        List<Genre> genres,
        List<Tag> tags,
        Optional<LanguageInfo> language,
        Optional<LanguageInfo> originalLanguage,
        Optional<string> publisher,
        Optional<int> pageCount)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Errors.Metadata.TitleCannotBeEmpty;
        if (releaseInfo is null)
            return Errors.Metadata.ReleaseInfoCannotBeNull;
        if (genres is null)
            return Errors.Metadata.GenresListCannotBeNull;
        if (tags is null)
            return Errors.Metadata.TagsListCannotBeNull;
        return new WrittenContentMetadata(
            title,
            originalTitle,
            description,
            releaseInfo,
            genres, tags,
            language,
            originalLanguage,
            publisher,
            pageCount
        );
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        foreach (object component in base.GetEqualityComponents())
            yield return component;
        yield return Publisher;
        yield return PageCount;
    }
}
