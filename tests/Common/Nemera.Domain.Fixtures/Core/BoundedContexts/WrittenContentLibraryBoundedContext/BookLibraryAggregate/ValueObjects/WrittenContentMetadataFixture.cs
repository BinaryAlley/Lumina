#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="WrittenContentMetadata"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentMetadataFixture
{
    private readonly Faker _faker = new();
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="WrittenContentMetadata"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the written content.</param>
    /// <param name="originalTitle">Optional. The original title of the written content.</param>
    /// <param name="description">Optional. The description of the written content.</param>
    /// <param name="releaseInfo">Optional. The release information of the written content.</param>
    /// <param name="genres">Optional. The genres of the written content.</param>
    /// <param name="tags">Optional. The tags associated with the written content.</param>
    /// <param name="language">Optional. The language of the written content.</param>
    /// <param name="originalLanguage">Optional. The original language of the written content.</param>
    /// <param name="publisher">Optional. The publisher of the written content.</param>
    /// <param name="pageCount">Optional. The number of pages in the written content.</param>
    /// <returns>The created <see cref="WrittenContentMetadata"/>.</returns>
    public WrittenContentMetadata Create(
        string? title = null,
        Optional<string>? originalTitle = null,
        Optional<string>? description = null,
        ReleaseInfo? releaseInfo = null,
        List<Genre>? genres = null,
        List<Tag>? tags = null,
        Optional<LanguageInfo>? language = null,
        Optional<LanguageInfo>? originalLanguage = null,
        Optional<string>? publisher = null,
        Optional<int>? pageCount = null)
    {
        return WrittenContentMetadata.Create(
            title ?? _faker.Commerce.ProductName(),
            originalTitle ?? Optional<string>.None(),
            description ?? Optional<string>.Some(_faker.Lorem.Sentence()),
            releaseInfo ?? _releaseInfoFixture.Create(),
            genres ?? [_genreFixture.Create(), _genreFixture.Create()],
            tags ?? [_tagFixture.Create(), _tagFixture.Create()],
            language ?? Optional<LanguageInfo>.Some(_languageInfoFixture.Create()),
            originalLanguage ?? Optional<LanguageInfo>.None(),
            publisher ?? Optional<string>.Some(_faker.Company.CompanyName()),
            pageCount ?? Optional<int>.Some(_faker.Random.Int(1, 1000))).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="WrittenContentMetadata"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="WrittenContentMetadata"/> instances.</returns>
    public List<WrittenContentMetadata> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
