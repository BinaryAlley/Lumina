#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="ShuffleTrackRowDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTrackRowDtoMappingTests
{
    private readonly ShuffleTrackRowFixture _shuffleTrackRowFixture = new();

    [Fact]
    public void ToResponse_WhenMappingTrackWithCoverPath_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        ShuffleTrackRowDto readModel = _shuffleTrackRowFixture.Create(
            title: "Bohemian Rhapsody",
            durationInSeconds: 355,
            albumTitle: "A Night at the Opera",
            coverPath: "a-night-at-the-opera-cover.jpg");

        // Act
        ShuffleTrackResponse result = readModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(readModel.TrackId, result.TrackId);
        Assert.Equal(readModel.Title, result.Title);
        Assert.Equal(readModel.DurationInSeconds, result.DurationInSeconds);
        Assert.Equal(readModel.AlbumId, result.AlbumId);
        Assert.Equal(readModel.AlbumTitle, result.AlbumTitle);
        Assert.Equal(readModel.ArtistId, result.ArtistId);
        Assert.Equal("a-night-at-the-opera-cover.jpg", result.CoverPath);
    }

    [Fact]
    public void ToResponse_WhenCoverPathIsMissing_ShouldMapNullCoverPath()
    {
        // Arrange
        ShuffleTrackRowDto readModel = _shuffleTrackRowFixture.Create(includeCoverPath: false);

        // Act
        ShuffleTrackResponse result = readModel.ToResponse();

        // Assert
        Assert.Null(result.CoverPath);
        Assert.Equal(readModel.TrackId, result.TrackId);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleRows_ShouldMapEveryRowAndPreserveTheOrder()
    {
        // Arrange
        List<ShuffleTrackRowDto> readModels = _shuffleTrackRowFixture.CreateMany(4);

        // Act
        IReadOnlyList<ShuffleTrackResponse> results = readModels.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(readModels.Count, results.Count);
        Assert.Equal(readModels.Select(readModel => readModel.TrackId), results.Select(response => response.TrackId));
        Assert.Equal(readModels.Select(readModel => readModel.Title), results.Select(response => response.Title));
        Assert.Equal(readModels.Select(readModel => readModel.DurationInSeconds), results.Select(response => response.DurationInSeconds));
        Assert.Equal(readModels.Select(readModel => readModel.AlbumId), results.Select(response => response.AlbumId));
        Assert.Equal(readModels.Select(readModel => readModel.AlbumTitle), results.Select(response => response.AlbumTitle));
        Assert.Equal(readModels.Select(readModel => readModel.ArtistId), results.Select(response => response.ArtistId));
        Assert.Equal(readModels.Select(readModel => readModel.CoverPath), results.Select(response => response.CoverPath));
    }

    [Fact]
    public void ToResponses_WhenInputIsEmpty_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        IEnumerable<ShuffleTrackRowDto> readModels = [];

        // Act
        IReadOnlyList<ShuffleTrackResponse> results = readModels.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }
}
