#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.Repositories.MusicLibrary.Specifications;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MusicLibrary.Specifications;

/// <summary>
/// Contains unit tests for the <see cref="ArtistSearchSpecification"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistSearchSpecificationTests
{
    private readonly ArtistEntityFixture _artistEntityFixture = new();

    [Fact]
    public void ToExpression_WhenNameContainsSearchTerm_ShouldMatchArtist()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(name: "Queens of the Stone Age", includeAlbums: false, includeContributors: false);
        ArtistSearchSpecification specification = new("Stone");

        // Act
        bool result = specification.IsSatisfiedBy(artist);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToExpression_WhenNameDoesNotContainSearchTerm_ShouldNotMatchArtist()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(name: "Radiohead", includeAlbums: false, includeContributors: false);
        ArtistSearchSpecification specification = new("Queen");

        // Act
        bool result = specification.IsSatisfiedBy(artist);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToExpression_WhenNameMatchesInDifferentCase_ShouldMatchCaseInsensitively()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(name: "Queens of the Stone Age", includeAlbums: false, includeContributors: false);
        ArtistSearchSpecification specification = new("QUEENS");

        // Act
        bool result = specification.IsSatisfiedBy(artist);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToExpression_WhenSearchTermIsEmpty_ShouldMatchEveryArtist()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(name: "Radiohead", includeAlbums: false, includeContributors: false);
        ArtistSearchSpecification specification = new(string.Empty);

        // Act
        bool result = specification.IsSatisfiedBy(artist);

        // Assert
        // The repository guards blank terms with IsNullOrWhiteSpace before building the specification, so an empty term is documented as matching every artist rather than being treated as a literal.
        Assert.True(result);
    }

    [Fact]
    public void ToExpression_WhenSearchTermIsWhitespace_ShouldNotMatchAnArtistWithoutThatWhitespace()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(name: "Radiohead", includeAlbums: false, includeContributors: false);
        ArtistSearchSpecification specification = new("   ");

        // Act
        bool result = specification.IsSatisfiedBy(artist);

        // Assert
        // A whitespace-only term is not silently ignored, it is matched literally.
        Assert.False(result);
    }

    [Fact]
    public void ToExpression_WhenApplied_ShouldFilterOutNonMatchingArtists()
    {
        // Arrange
        List<ArtistEntity> artists =
        [
            _artistEntityFixture.Create(name: "Queens of the Stone Age", includeAlbums: false, includeContributors: false),
            _artistEntityFixture.Create(name: "Radiohead", includeAlbums: false, includeContributors: false),
            _artistEntityFixture.Create(name: "Stone Temple Pilots", includeAlbums: false, includeContributors: false)
        ];
        ArtistSearchSpecification specification = new("stone");

        // Act
        IEnumerable<ArtistEntity> matchingArtists = artists.AsQueryable().Where(specification.ToExpression());

        // Assert
        Assert.Equal(2, matchingArtists.Count());
        Assert.DoesNotContain(matchingArtists, artist => artist.Name == "Radiohead");
    }
}
