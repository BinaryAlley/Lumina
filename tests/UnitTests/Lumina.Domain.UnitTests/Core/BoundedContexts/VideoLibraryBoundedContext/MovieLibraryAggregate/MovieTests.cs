#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Movie"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovieTests
{
    private readonly VideoMetadataFixture _videoMetadataFixture = new();
    private readonly MovieIdFixture _movieIdFixture = new();
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();
    private readonly MovieFixture _movieFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateMovieWithGeneratedId()
    {
        // Arrange
        VideoMetadata metadata = _videoMetadataFixture.Create();
        List<MediaContributorId> contributors = [_mediaContributorIdFixture.Create()];
        List<Rating> ratings = [];

        // Act
        Result<Movie> result = Movie.Create(metadata, contributors, ratings);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(metadata, result.Value.Metadata);
        Assert.Single(result.Value.Contributors);
        Assert.Empty(result.Value.Ratings);
        Assert.NotEqual(default, result.Value.Id.Value);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingId_ShouldCreateMovieWithThatId()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        VideoMetadata metadata = _videoMetadataFixture.Create();
        List<MediaContributorId> contributors = [];
        List<Rating> ratings = [];

        // Act
        Result<Movie> result = Movie.Create(_movieIdFixture.Create(id), metadata, contributors, ratings);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id.Value);
        Assert.Equal(metadata, result.Value.Metadata);
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        MovieId id = _movieIdFixture.Create();
        VideoMetadata metadata = _videoMetadataFixture.Create();
        List<MediaContributorId> contributors = [_mediaContributorIdFixture.Create(), _mediaContributorIdFixture.Create()];
        List<Rating> ratings = [CreateRating(4.5m)];

        // Act
        Movie movie = new(id, metadata, contributors, ratings);

        // Assert
        Assert.Equal(id, movie.Id);
        Assert.Equal(metadata, movie.Metadata);
        Assert.Equal(2, movie.Contributors.Count);
        Assert.Single(movie.Ratings);
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Movie firstMovie = _movieFixture.Create(id: _movieIdFixture.Create(id));
        Movie secondMovie = _movieFixture.Create(id: _movieIdFixture.Create(id));

        // Act
        bool result = firstMovie.Equals(secondMovie);

        // Assert
        Assert.True(result);
    }

    private static Rating CreateRating(decimal value)
    {
        return new TestRating(value, 10);
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="Rating"/> class.
    /// </summary>
    private sealed class TestRating : Rating
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRating"/> class.
        /// </summary>
        /// <param name="value">The numeric value of the rating.</param>
        /// <param name="maxValue">The maximum possible rating value.</param>
        public TestRating(decimal value, decimal maxValue) : base(value, maxValue, Optional<int>.None())
        {
        }

        /// <summary>
        /// Gets the list of items that define equality of the object.
        /// </summary>
        /// <returns>A list of items defining the equality.</returns>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return MaxValue;
            yield return VoteCount;
        }
    }
}
