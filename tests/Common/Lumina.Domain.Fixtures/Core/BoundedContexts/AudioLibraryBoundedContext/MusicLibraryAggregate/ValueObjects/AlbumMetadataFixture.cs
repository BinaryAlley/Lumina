#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="AlbumMetadata"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumMetadataFixture
{
    private readonly Faker _faker = new();
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumMetadata"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="originalTitle">Optional. The original title of the album.</param>
    /// <param name="description">Optional. The description of the album.</param>
    /// <param name="releaseInfo">Optional. The release information of the album.</param>
    /// <param name="genres">Optional. The genres of the album.</param>
    /// <param name="tags">Optional. The tags associated with the album.</param>
    /// <param name="language">Optional. The language of the album.</param>
    /// <param name="originalLanguage">Optional. The original language of the album.</param>
    /// <param name="releaseType">Optional. The type of the release.</param>
    /// <param name="releaseStatus">Optional. The status of the release.</param>
    /// <param name="totalDiscs">Optional. The number of discs of the release.</param>
    /// <param name="totalTracks">Optional. The number of tracks of the release.</param>
    /// <returns>The created <see cref="AlbumMetadata"/>.</returns>
    public AlbumMetadata Create(
        string? title = null,
        Optional<string>? originalTitle = null,
        Optional<string>? description = null,
        ReleaseInfo? releaseInfo = null,
        List<Genre>? genres = null,
        List<Tag>? tags = null,
        Optional<LanguageInfo>? language = null,
        Optional<LanguageInfo>? originalLanguage = null,
        Optional<MusicReleaseType>? releaseType = null,
        Optional<MusicReleaseStatus>? releaseStatus = null,
        Optional<int>? totalDiscs = null,
        int? totalTracks = null)
    {
        Result<AlbumMetadata> metadataResult = AlbumMetadata.Create(
            title ?? _faker.Commerce.ProductName(),
            originalTitle ?? Optional<string>.None(),
            description ?? Optional<string>.Some(_faker.Lorem.Sentence()),
            releaseInfo ?? _releaseInfoFixture.Create(),
            genres ?? [_genreFixture.Create(), _genreFixture.Create()],
            tags ?? [_tagFixture.Create(), _tagFixture.Create()],
            language ?? Optional<LanguageInfo>.Some(_languageInfoFixture.Create()),
            originalLanguage ?? Optional<LanguageInfo>.None(),
            releaseType ?? Optional<MusicReleaseType>.Some(_faker.PickRandom<MusicReleaseType>()),
            releaseStatus ?? Optional<MusicReleaseStatus>.Some(_faker.PickRandom<MusicReleaseStatus>()),
            totalDiscs ?? Optional<int>.Some(_faker.Random.Int(1, 4)),
            totalTracks ?? _faker.Random.Int(1, 50));

        if (metadataResult.IsFailure)
            throw new InvalidOperationException("Failed to create AlbumMetadata: " + string.Join(", ", metadataResult.Errors));
        return metadataResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="AlbumMetadata"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AlbumMetadata"/> instances.</returns>
    public List<AlbumMetadata> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
