#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.ValueObjects.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="BaseMetadata"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BaseMetadataTests
{
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    [Fact]
    public void Constructor_WhenCalledWithValidData_ShouldSetAllProperties()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();
        List<Genre> genres = [_genreFixture.Create(name: "Science Fiction")];
        List<Tag> tags = [_tagFixture.Create(name: "bestseller")];
        Optional<LanguageInfo> language = Optional<LanguageInfo>.Some(_languageInfoFixture.Create());

        // Act
        TestMetadata metadata = new(
            "Dune",
            Optional<string>.Some("Dune (Original)"),
            Optional<string>.Some("A classic novel."),
            releaseInfo,
            genres,
            tags,
            language,
            Optional<LanguageInfo>.None());

        // Assert
        Assert.Equal("Dune", metadata.Title);
        Assert.Equal("Dune (Original)", metadata.OriginalTitle.Value);
        Assert.Equal("A classic novel.", metadata.Description.Value);
        Assert.Equal(releaseInfo, metadata.ReleaseInfo);
        Assert.Single(metadata.Genres);
        Assert.Single(metadata.Tags);
        Assert.True(metadata.Language.HasValue);
        Assert.False(metadata.OriginalLanguage.HasValue);
    }

    [Fact]
    public void Constructor_WhenTitleIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestMetadata(
            null!,
            Optional<string>.None(),
            Optional<string>.None(),
            _releaseInfoFixture.Create(),
            [],
            [],
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None()));
    }

    [Fact]
    public void Equals_WithSameData_ShouldReturnTrue()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();
        List<Genre> genres = [_genreFixture.Create(name: "Science Fiction")];
        List<Tag> tags = [_tagFixture.Create(name: "bestseller")];

        // Act
        TestMetadata firstMetadata = new("Dune", Optional<string>.None(), Optional<string>.None(), releaseInfo, genres, tags, Optional<LanguageInfo>.None(), Optional<LanguageInfo>.None());
        TestMetadata secondMetadata = new("Dune", Optional<string>.None(), Optional<string>.None(), releaseInfo, genres, tags, Optional<LanguageInfo>.None(), Optional<LanguageInfo>.None());

        // Assert
        Assert.Equal(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentTitle_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        TestMetadata firstMetadata = new("Dune", Optional<string>.None(), Optional<string>.None(), releaseInfo, [], [], Optional<LanguageInfo>.None(), Optional<LanguageInfo>.None());
        TestMetadata secondMetadata = new("Foundation", Optional<string>.None(), Optional<string>.None(), releaseInfo, [], [], Optional<LanguageInfo>.None(), Optional<LanguageInfo>.None());

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="BaseMetadata"/> class.
    /// </summary>
    private sealed class TestMetadata : BaseMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMetadata"/> class.
        /// </summary>
        /// <param name="title">The title of the media item.</param>
        /// <param name="originalTitle">The optional original title.</param>
        /// <param name="description">The optional description.</param>
        /// <param name="releaseInfo">The release information.</param>
        /// <param name="genres">The genres.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="language">The optional language.</param>
        /// <param name="originalLanguage">The optional original language.</param>
        public TestMetadata(
            string title,
            Optional<string> originalTitle,
            Optional<string> description,
            ReleaseInfo releaseInfo,
            List<Genre> genres,
            List<Tag> tags,
            Optional<LanguageInfo> language,
            Optional<LanguageInfo> originalLanguage)
            : base(title, originalTitle, description, releaseInfo, genres, tags, language, originalLanguage)
        {
        }

        /// <summary>
        /// Gets the list of items that define equality of the object.
        /// </summary>
        /// <returns>A list of items defining the equality.</returns>
        public override IEnumerable<object> GetEqualityComponents()
        {
            return base.GetEqualityComponents();
        }
    }
}
