#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="WrittenContentMetadata"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentMetadataTests
{
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();
    private readonly WrittenContentMetadataFixture _writtenContentMetadataFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateMetadataWithAllPropertiesSet()
    {
        // Act
        Result<WrittenContentMetadata> result = WrittenContentMetadata.Create(
            "Dune",
            Optional<string>.Some("Dune (Original)"),
            Optional<string>.Some("A classic science fiction novel."),
            _releaseInfoFixture.Create(),
            [_genreFixture.Create(name: "Science Fiction")],
            [_tagFixture.Create(name: "classic")],
            Optional<LanguageInfo>.Some(_languageInfoFixture.Create()),
            Optional<LanguageInfo>.None(),
            Optional<string>.Some("Chilton Books"),
            Optional<int>.Some(412));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Dune", result.Value.Title);
        Assert.Equal("Dune (Original)", result.Value.OriginalTitle.Value);
        Assert.Equal("A classic science fiction novel.", result.Value.Description.Value);
        Assert.Single(result.Value.Genres);
        Assert.Single(result.Value.Tags);
        Assert.True(result.Value.Language.HasValue);
        Assert.False(result.Value.OriginalLanguage.HasValue);
        Assert.True(result.Value.Publisher.HasValue);
        Assert.Equal("Chilton Books", result.Value.Publisher.Value);
        Assert.True(result.Value.PageCount.HasValue);
        Assert.Equal(412, result.Value.PageCount.Value);
    }

    [Fact]
    public void Create_WhenCalledWithOptionalValuesAbsent_ShouldCreateMetadataWithoutThem()
    {
        // Act
        Result<WrittenContentMetadata> result = WrittenContentMetadata.Create(
            "Dune",
            Optional<string>.None(),
            Optional<string>.None(),
            _releaseInfoFixture.Create(),
            [],
            [],
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None(),
            Optional<string>.None(),
            Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.OriginalTitle.HasValue);
        Assert.False(result.Value.Publisher.HasValue);
        Assert.False(result.Value.PageCount.HasValue);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        WrittenContentMetadata firstResult = _writtenContentMetadataFixture.Create(
            title: "Dune",
            originalTitle: Optional<string>.None(),
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            publisher: Optional<string>.None(),
            pageCount: Optional<int>.None());
        WrittenContentMetadata secondResult = _writtenContentMetadataFixture.Create(
            title: "Dune",
            originalTitle: Optional<string>.None(),
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            publisher: Optional<string>.None(),
            pageCount: Optional<int>.None());

        // Assert
        Assert.Equal(firstResult, secondResult);
    }

    [Fact]
    public void Equals_WithDifferentPublisher_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        WrittenContentMetadata firstResult = _writtenContentMetadataFixture.Create(
            title: "Dune",
            originalTitle: Optional<string>.None(),
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            publisher: Optional<string>.None(),
            pageCount: Optional<int>.None());
        WrittenContentMetadata secondResult = _writtenContentMetadataFixture.Create(
            title: "Dune",
            originalTitle: Optional<string>.None(),
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            publisher: Optional<string>.Some("Other Publisher"),
            pageCount: Optional<int>.None());

        // Assert
        Assert.NotEqual(firstResult, secondResult);
    }
}
