#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="AddAlbumCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddAlbumCommandMappingTests
{
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create();

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Album album = result.Value;
        Assert.Equal(command.Metadata!.Title, album.Metadata.Title);
        Assert.Equal(command.MediaFormat, album.MediaFormat.Value);
        Assert.Equal(command.Barcode, album.Barcode.Value.Value);
        Assert.Equal(command.CatalogNumber, album.CatalogNumber.Value);
        Assert.Equal(command.MusicBrainzReleaseId, album.MusicBrainzReleaseId.Value.Value);
        Assert.Equal(command.MusicBrainzReleaseGroupId, album.MusicBrainzReleaseGroupId.Value.Value);
        Assert.Equal(command.MusicBrainzReleaseArtistId, album.MusicBrainzReleaseArtistId.Value.Value);
        Assert.Equal(command.Contributors!.Count, album.Contributors.Count);
        Assert.Equal(command.Ratings!.Count, album.Ratings.Count);
        Assert.Equal(command.Tracks!.Count, album.Tracks.Count);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumIdIsProvided_ShouldReuseTheProvidedAlbumId()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        AddAlbumCommand command = _addAlbumCommandFixture.Create(albumId: albumId);

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(albumId, result.Value.Id.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumIdIsNotProvided_ShouldMintANewAlbumId()
    {
        // Arrange
        AddAlbumCommand firstCommand = _addAlbumCommandFixture.Create();
        AddAlbumCommand secondCommand = _addAlbumCommandFixture.Create();

        // Act
        Result<Album> firstResult = firstCommand.ToDomainEntity();
        Result<Album> secondResult = secondCommand.ToDomainEntity();

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        // Two independent mappings must mint two distinct album Ids, instead of both collapsing to the same default value.
        Assert.NotEqual(firstResult.Value.Id.Value, secondResult.Value.Id.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNones()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(
            includeMediaFormat: false,
            includeBarcode: false,
            includeCatalogNumber: false,
            includeMusicBrainzReleaseId: false,
            includeMusicBrainzReleaseGroupId: false,
            includeMusicBrainzReleaseArtistId: false,
            contributors: [],
            ratings: [],
            tracks: []);

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Album album = result.Value;
        Assert.Equal(command.Metadata!.Title, album.Metadata.Title);
        Assert.False(album.MediaFormat.HasValue);
        Assert.False(album.Barcode.HasValue);
        Assert.False(album.CatalogNumber.HasValue);
        Assert.False(album.MusicBrainzReleaseId.HasValue);
        Assert.False(album.MusicBrainzReleaseGroupId.HasValue);
        Assert.False(album.MusicBrainzReleaseArtistId.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataCreationFails_ShouldReturnError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(
            metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenBarcodeCannotBeEmpty_ShouldReturnError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(barcode: string.Empty);

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.BarcodeValueCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenBarcodeHasInvalidFormat_ShouldReturnError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(barcode: "not-a-barcode");

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.InvalidFormatForBarcode, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenRatingCreationFails_ShouldReturnError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenTrackCreationFails_ShouldReturnError()
    {
        // Arrange
        AddTrackCommand invalidTrack = _addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(value: "invalid")]);
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [invalidTrack]);

        // Act
        Result<Album> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Music.IsrcInvalidFormat.Description);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleCommands_ShouldMapEachCommand()
    {
        // Arrange
        List<AddAlbumCommand> commands = _addAlbumCommandFixture.CreateMany(3);

        // Act
        List<Result<Album>> results = [.. commands.ToDomainEntities()];

        // Assert
        Assert.Equal(commands.Count, results.Count);
        Assert.All(results, result => Assert.False(result.IsFailure));
        for (int i = 0; i < commands.Count; i++)
        {
            Assert.Equal(commands[i].Metadata!.Title, results[i].Value.Metadata.Title);
            Assert.Equal(commands[i].Tracks!.Count, results[i].Value.Tracks.Count);
        }
    }
}
