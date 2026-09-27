#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="ArtistEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistEntityMappingTests
{
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly PaginatedResultDtoFixture<ArtistEntity> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingValidArtistEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        ArtistEntity entity = _artistEntityFixture.Create();

        // Act
        Result<Artist> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Artist artist = result.Value;
        Assert.Equal(entity.Id, artist.Id.Value);
        Assert.Equal(entity.LibraryId, artist.LibraryId.Value);
        Assert.Equal(entity.Name, artist.Name);
        Assert.True(artist.Website.HasValue);
        Assert.Equal(entity.Website, artist.Website.Value);
        Assert.True(artist.MusicBrainzArtistId.HasValue);
        Assert.Equal(entity.MusicBrainzArtistId, artist.MusicBrainzArtistId.Value.Value);
        Assert.Equal(entity.Contributors.Select(contributor => contributor.MediaContributorId), artist.Contributors.Select(contributor => contributor.ContributorId.Value));
        Assert.Equal(entity.Contributors.Select(contributor => contributor.Role), artist.Contributors.Select(contributor => contributor.Role));
        Assert.Equal(entity.Albums.Count, artist.Albums.Count);
        Assert.Equal(entity.Albums.Select(album => album.Id), artist.Albums.Select(album => album.Id.Value));
        Assert.Equal(entity.Albums.Select(album => album.Title), artist.Albums.Select(album => album.Metadata.Title));
        Assert.Equal(entity.CreatedOnUtc, artist.CreatedOnUtc);
        Assert.True(artist.UpdatedOnUtc.HasValue);
        Assert.Equal(entity.UpdatedOnUtc, artist.UpdatedOnUtc.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AlbumEntity invalidAlbum = _albumEntityFixture.Create(
            libraryId: libraryId,
            originalReleaseDate: new DateOnly(2000, 1, 1),
            originalReleaseYear: 1999);
        ArtistEntity entity = _artistEntityFixture.Create(libraryId: libraryId, albums: [invalidAlbum]);

        // Act
        Result<Artist> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNones()
    {
        // Arrange
        ArtistEntity entity = _artistEntityFixture.Create(
            includeMetadata: false,
            includeWebsite: false,
            includeMusicBrainzArtistId: false,
            includeContributors: false);

        // Act
        Result<Artist> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Artist artist = result.Value;
        Assert.False(artist.Website.HasValue);
        Assert.False(artist.MusicBrainzArtistId.HasValue);
        Assert.Empty(artist.Contributors);
        Assert.Single(artist.Albums);
    }

    [Fact]
    public void ToResponse_WhenMappingArtistEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        ArtistEntity entity = _artistEntityFixture.Create();

        // Act
        ArtistResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.LibraryId, result.LibraryId);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Website, result.Website);
        Assert.Equal(entity.MusicBrainzArtistId, result.MusicBrainzArtistId);
        Assert.Equal(entity.Contributors.Select(contributor => new MediaContributorReferenceDto(contributor.MediaContributorId, contributor.Role)), result.Contributors);
        Assert.Equal(entity.Albums.Count, result.Albums!.Count);
        Assert.Equal(entity.Albums.Select(album => album.Id), result.Albums.Select(album => album.Id));
        Assert.Equal(entity.Albums.Select(album => album.Title), result.Albums.Select(album => album.Metadata.Title));
        Assert.Equal(entity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, result.UpdatedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenOptionalPropertiesAreMissing_ShouldMapNullsAndEmptyCollections()
    {
        // Arrange
        ArtistEntity entity = _artistEntityFixture.Create(
            includeMetadata: false,
            includeWebsite: false,
            includeMusicBrainzArtistId: false,
            includeContributors: false);

        // Act
        ArtistResponse result = entity.ToResponse();

        // Assert
        Assert.Null(result.Website);
        Assert.Null(result.MusicBrainzArtistId);
        Assert.Empty(result.Contributors!);
        Assert.Single(result.Albums!);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleArtistEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<ArtistEntity> entities = _artistEntityFixture.CreateMany(2);

        // Act
        IReadOnlyList<ArtistResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entities[i].Id, results[i].Id);
            Assert.Equal(entities[i].Name, results[i].Name);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingPaginatedArtistEntities_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        List<ArtistEntity> entities = _artistEntityFixture.CreateMany(2);
        PaginatedResultDto<ArtistEntity> paginatedEntities = _paginatedResultDtoFixture.Create(
            data: entities,
            currentPage: 1,
            perPage: 10,
            count: 2,
            numberOfPages: 1);

        // Act
        PaginatedResponse<ArtistResponse> result = paginatedEntities.ToResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paginatedEntities.Data.Count, result.Data.Count);
        Assert.Equal(paginatedEntities.CurrentPage, result.CurrentPage);
        Assert.Equal(paginatedEntities.PerPage, result.PerPage);
        Assert.Equal(paginatedEntities.Count, result.Count);
        Assert.Equal(paginatedEntities.NumberOfPages, result.NumberOfPages);
    }
}
