#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
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

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="AddTrackCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackCommandMappingTests
{
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();
    private readonly MusicTrackMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();

        // Act
        Result<Track> result = command.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.Path, result.Value.Path);
        Assert.Equal(command.Metadata!.Title, result.Value.Metadata.Title);
        Assert.Equal(command.TrackNumber!.Value, result.Value.TrackNumber);
        Assert.Equal(command.DiscNumber, result.Value.DiscNumber.Value);
        Assert.Equal(command.Script, result.Value.Script.Value);
        Assert.Equal(command.Key, result.Value.Key.Value);
        Assert.Equal(command.Bpm, result.Value.Bpm.Value);
        Assert.Equal(command.Work!.Title, result.Value.Work.Value.Title);
        Assert.Equal(command.MusicBrainzRecordingId, result.Value.MusicBrainzRecordingId.Value.Value);
        Assert.Equal(command.MusicBrainzTrackId, result.Value.MusicBrainzTrackId.Value.Value);
        Assert.Equal(command.Work!.MusicBrainzWorkId, result.Value.Work.Value.MusicBrainzWorkId.Value);
        Assert.Equal(command.Moods!.Count, result.Value.Moods.Count);
        Assert.Equal(command.Isrcs!.Count, result.Value.Isrcs.Count);
        Assert.Equal(command.Contributors!.Count, result.Value.Contributors.Count);
        Assert.Equal(command.Ratings!.Count, result.Value.Ratings.Count);
    }

    [Fact]
    public void ToDomainEntity_WhenTrackIdIsProvided_ShouldReuseTheProvidedTrackId()
    {
        // Arrange
        Guid trackId = Guid.NewGuid();
        AddTrackCommand command = _addTrackCommandFixture.Create(trackId: trackId);

        // Act
        Result<Track> result = command.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(trackId, result.Value.Id.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenTrackIdIsNotProvided_ShouldMintANewTrackId()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();

        // Act
        Result<Track> result = command.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value.Id.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataCreationFails_ShouldReturnError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        Result<Track> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenMoodCreationFails_ShouldReturnError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(includeName: false)]);

        // Act
        Result<Track> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.MoodNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenIsrcCreationFails_ShouldReturnError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(value: "invalid")]);

        // Act
        Result<Track> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Music.IsrcInvalidFormat.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenRatingCreationFails_ShouldReturnError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        Result<Track> result = command.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleCommands_ShouldMapEachCommand()
    {
        // Arrange
        List<AddTrackCommand> commands = _addTrackCommandFixture.CreateMany(3);

        // Act
        List<Result<Track>> results = [.. commands.ToDomainEntities()];

        // Assert
        Assert.Equal(commands.Count, results.Count);
        Assert.All(results, result => Assert.False(result.IsFailure));
        for (int i = 0; i < commands.Count; i++)
        {
            Assert.Equal(commands[i].Path, results[i].Value.Path);
            Assert.Equal(commands[i].Metadata!.Title, results[i].Value.Metadata.Title);
        }
    }
}
