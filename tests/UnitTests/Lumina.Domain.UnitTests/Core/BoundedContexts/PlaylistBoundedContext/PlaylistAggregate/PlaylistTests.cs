#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Playlist"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistTests
{
    private readonly PlaylistFixture _playlistFixture = new();
    private readonly PlaylistIdFixture _playlistIdFixture = new();
    private readonly PlaylistEntryFixture _playlistEntryFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    [Fact]
    public void Create_WhenCalled_ShouldCreateAPlaylistWithoutEntriesAndRaiseTheCreatedEvent()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        DateTime beforeUtc = DateTime.UtcNow;

        // Act
        Result<Playlist> result = Playlist.Create(userId, "Morning", Optional<string>.Some("Wake up slowly"), PlaylistType.Music);

        // Assert
        Assert.False(result.IsFailure);
        Playlist playlist = result.Value;
        Assert.NotEqual(Guid.Empty, playlist.Id.Value);
        Assert.Equal(userId, playlist.UserId);
        Assert.Equal("Morning", playlist.Name);
        Assert.True(playlist.Description.HasValue);
        Assert.Equal("Wake up slowly", playlist.Description.Value);
        Assert.Equal(PlaylistType.Music, playlist.Type);
        Assert.False(playlist.IsDeleted);
        Assert.Empty(playlist.Entries);
        Assert.InRange(playlist.CreatedOnUtc, beforeUtc, DateTime.UtcNow);
        Assert.False(playlist.UpdatedOnUtc.HasValue);
        PlaylistCreatedDomainEvent createdEvent = Assert.IsType<PlaylistCreatedDomainEvent>(Assert.Single(playlist.GetDomainEvents()));
        Assert.NotEqual(Guid.Empty, createdEvent.Id);
        Assert.Equal(playlist.Id, createdEvent.PlaylistId);
        Assert.Equal(userId, createdEvent.UserId);
        Assert.Equal("Morning", createdEvent.Name);
        Assert.Equal(PlaylistType.Music, createdEvent.Type);
        Assert.Equal(playlist.CreatedOnUtc, createdEvent.OccurredOnUtc);
    }

    [Fact]
    public void Create_WhenDescriptionIsNotSupplied_ShouldCreateThePlaylistWithoutADescription()
    {
        // Act
        Result<Playlist> result = Playlist.Create(_userIdFixture.Create(), "Morning", Optional<string>.None(), PlaylistType.Music);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Description.HasValue);
        Assert.Single(result.Value.GetDomainEvents());
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();

        // Act
        Result<Playlist> firstResult = Playlist.Create(userId, "Morning", Optional<string>.None(), PlaylistType.Music);
        Result<Playlist> secondResult = Playlist.Create(userId, "Morning", Optional<string>.None(), PlaylistType.Music);

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.Id, secondResult.Value.Id);
    }

    [Fact]
    public void Create_WhenRehydratingWithEntries_ShouldPreserveEveryMemberAndRaiseNoEvents()
    {
        // Arrange
        PlaylistId id = _playlistIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        List<PlaylistEntry> entries = _playlistEntryFixture.CreateMany(3);
        DateTime createdOnUtc = new(2024, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);

        // Act
        Result<Playlist> result = Playlist.Create(
            id,
            userId,
            "Commute",
            Optional<string>.Some("Train rides"),
            PlaylistType.Music,
            entries,
            createdOnUtc,
            Optional<DateTime>.Some(updatedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        Playlist playlist = result.Value;
        Assert.Equal(id, playlist.Id);
        Assert.Equal(userId, playlist.UserId);
        Assert.Equal("Commute", playlist.Name);
        Assert.True(playlist.Description.HasValue);
        Assert.Equal("Train rides", playlist.Description.Value);
        Assert.Equal(PlaylistType.Music, playlist.Type);
        Assert.Equal(createdOnUtc, playlist.CreatedOnUtc);
        Assert.True(playlist.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, playlist.UpdatedOnUtc.Value);
        Assert.False(playlist.IsDeleted);
        Assert.Equal(entries.Select(entry => entry.Id), playlist.Entries.Select(entry => entry.Id));
        Assert.Empty(playlist.GetDomainEvents());
    }

    [Fact]
    public void Create_WhenRehydratingWithoutUpdatedOnUtcOrDescription_ShouldLeaveBothWithoutAValue()
    {
        // Arrange
        DateTime createdOnUtc = new(2024, 2, 3, 4, 5, 6, DateTimeKind.Utc);

        // Act
        Result<Playlist> result = Playlist.Create(
            _playlistIdFixture.Create(),
            _userIdFixture.Create(),
            "Commute",
            Optional<string>.None(),
            PlaylistType.Music,
            [],
            createdOnUtc,
            Optional<DateTime>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Description.HasValue);
        Assert.False(result.Value.UpdatedOnUtc.HasValue);
        Assert.Empty(result.Value.Entries);
        Assert.Empty(result.Value.GetDomainEvents());
    }

    [Theory]
    [InlineData(PlaylistMediaItemType.VideoFile)] // a video file cannot belong to a music playlist
    [InlineData((PlaylistMediaItemType)999)] // a value outside the declared enumeration members
    public void Create_WhenAnEntryReferencesAMediaItemTypeThePlaylistCannotContain_ShouldReturnPlaylistDoesNotAcceptMediaItems(PlaylistMediaItemType mediaItemType)
    {
        // Arrange
        List<PlaylistEntry> entries = [_playlistEntryFixture.Create(mediaItemType: mediaItemType)];

        // Act
        Result<Playlist> result = Playlist.Create(
            _playlistIdFixture.Create(),
            _userIdFixture.Create(),
            "Commute",
            Optional<string>.None(),
            PlaylistType.Music,
            entries,
            DateTime.UtcNow,
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playlists.PlaylistDoesNotAcceptMediaItems, result.FirstError);
    }

    [Fact]
    public void Create_WhenThePlaylistTypeIsNotDefined_ShouldReturnPlaylistDoesNotAcceptMediaItems()
    {
        // Arrange
        List<PlaylistEntry> entries = [_playlistEntryFixture.Create(mediaItemType: PlaylistMediaItemType.MusicTrack)];

        // Act
        Result<Playlist> result = Playlist.Create(
            _playlistIdFixture.Create(),
            _userIdFixture.Create(),
            "Commute",
            Optional<string>.None(),
            (PlaylistType)999,
            entries,
            DateTime.UtcNow,
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playlists.PlaylistDoesNotAcceptMediaItems, result.FirstError);
    }

    [Fact]
    public void Create_WhenTwoEntriesShareAPosition_ShouldReturnPlaylistEntryPositionsAreInvalid()
    {
        // Arrange
        List<PlaylistEntry> entries =
        [
            _playlistEntryFixture.Create(position: 1024),
            _playlistEntryFixture.Create(position: 1024),
        ];

        // Act
        Result<Playlist> result = Playlist.Create(
            _playlistIdFixture.Create(),
            _userIdFixture.Create(),
            "Commute",
            Optional<string>.None(),
            PlaylistType.Music,
            entries,
            DateTime.UtcNow,
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playlists.PlaylistEntryPositionsAreInvalid, result.FirstError);
    }

    [Fact]
    public void Create_WhenThePositionsAreSparseAndUnique_ShouldAcceptTheEntries()
    {
        // Arrange
        List<PlaylistEntry> entries =
        [
            _playlistEntryFixture.Create(position: 0),
            _playlistEntryFixture.Create(position: 1_048_576),
            _playlistEntryFixture.Create(position: long.MaxValue),
        ];

        // Act
        Result<Playlist> result = Playlist.Create(
            _playlistIdFixture.Create(),
            _userIdFixture.Create(),
            "Commute",
            Optional<string>.None(),
            PlaylistType.Music,
            entries,
            DateTime.UtcNow,
            Optional<DateTime>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([0L, 1_048_576L, long.MaxValue], result.Value.Entries.Select(entry => entry.Position));
    }

    [Fact]
    public void UpdateDetails_WhenTheNameChanges_ShouldApplyTheNewDetailsAndRaiseTheDetailsUpdatedEvent()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        Playlist playlist = _playlistFixture.Create(
            userId: userId,
            name: "Focus",
            description: "Deep work");
        DateTime beforeUtc = DateTime.UtcNow;

        // Act
        Result<Updated> result = playlist.UpdateDetails("Deep Focus", Optional<string>.Some("Long sessions"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal("Deep Focus", playlist.Name);
        Assert.True(playlist.Description.HasValue);
        Assert.Equal("Long sessions", playlist.Description.Value);
        PlaylistDetailsUpdatedDomainEvent updatedEvent = Assert.IsType<PlaylistDetailsUpdatedDomainEvent>(Assert.Single(playlist.GetDomainEvents()));
        Assert.NotEqual(Guid.Empty, updatedEvent.Id);
        Assert.Equal(playlist.Id, updatedEvent.PlaylistId);
        Assert.Equal(userId, updatedEvent.UserId);
        Assert.Equal("Deep Focus", updatedEvent.Name);
        Assert.InRange(updatedEvent.OccurredOnUtc, beforeUtc, DateTime.UtcNow);
    }

    [Fact]
    public void UpdateDetails_WhenOnlyTheDescriptionChanges_ShouldApplyItAndRaiseTheDetailsUpdatedEvent()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create(
            name: "Focus",
            description: "Deep work");

        // Act
        Result<Updated> result = playlist.UpdateDetails("Focus", Optional<string>.Some("Deeper work"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Focus", playlist.Name);
        Assert.Equal("Deeper work", playlist.Description.Value);
        Assert.Single(playlist.GetDomainEvents());
    }

    [Fact]
    public void UpdateDetails_WhenNothingChanges_ShouldReturnUpdatedWithoutRaisingAnEvent()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create(
            name: "Focus",
            description: "Deep work");

        // Act
        Result<Updated> result = playlist.UpdateDetails("Focus", Optional<string>.Some("Deep work"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal("Focus", playlist.Name);
        Assert.Equal("Deep work", playlist.Description.Value);
        Assert.Empty(playlist.GetDomainEvents());
    }

    [Fact]
    public void UpdateDetails_WhenTheDescriptionAppears_ShouldTreatItAsAChangeAndRaiseTheDetailsUpdatedEvent()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create(
            name: "Focus",
            includeDescription: false);

        // Act
        Result<Updated> result = playlist.UpdateDetails("Focus", Optional<string>.Some("Deep work"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(playlist.Description.HasValue);
        Assert.Equal("Deep work", playlist.Description.Value);
        Assert.Single(playlist.GetDomainEvents());
    }

    [Fact]
    public void UpdateDetails_WhenTheDescriptionDisappears_ShouldTreatItAsAChangeAndRaiseTheDetailsUpdatedEvent()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create(
            name: "Focus",
            description: "Deep work");

        // Act
        Result<Updated> result = playlist.UpdateDetails("Focus", Optional<string>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(playlist.Description.HasValue);
        Assert.Single(playlist.GetDomainEvents());
    }

    [Theory]
    [InlineData(PlaylistType.Music, PlaylistMediaItemType.MusicTrack, true)] // a music playlist accepts music tracks
    [InlineData(PlaylistType.Music, PlaylistMediaItemType.VideoFile, false)] // a music playlist does not accept video files
    [InlineData(PlaylistType.Music, (PlaylistMediaItemType)999, false)] // a value outside the declared enumeration members
    [InlineData((PlaylistType)999, PlaylistMediaItemType.MusicTrack, false)] // a playlist type outside the declared enumeration members
    [InlineData((PlaylistType)999, PlaylistMediaItemType.VideoFile, false)] // both sides outside the declared enumeration members
    [InlineData((PlaylistType)(-1), PlaylistMediaItemType.MusicTrack, false)] // a playlist type below the declared enumeration members
    [InlineData((PlaylistType)(-1), PlaylistMediaItemType.VideoFile, false)] // both sides below the declared enumeration members
    public void CanContain_WhenCalled_ShouldOnlyAcceptMusicTracksForMusicPlaylists(PlaylistType playlistType, PlaylistMediaItemType mediaItemType, bool expectedCanContain)
    {
        // Act
        bool canContain = Playlist.CanContain(playlistType, mediaItemType);

        // Assert
        Assert.Equal(expectedCanContain, canContain);
    }

    [Fact]
    public void Delete_WhenCalled_ShouldMarkThePlaylistAsDeletedAndRaiseOneEvent()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create(name: "Playlist");
        DateTime beforeUtc = DateTime.UtcNow;

        // Act
        Result<Deleted> result = playlist.Delete();
        List<IDomainEvent> domainEvents = playlist.GetDomainEvents();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.True(playlist.IsDeleted);
        PlaylistDeletedDomainEvent deletedEvent = Assert.IsType<PlaylistDeletedDomainEvent>(Assert.Single(domainEvents));
        Assert.Equal(playlist.Id, deletedEvent.PlaylistId);
        Assert.Equal(playlist.UserId, deletedEvent.UserId);
        Assert.InRange(deletedEvent.OccurredOnUtc, beforeUtc, DateTime.UtcNow);
    }

    [Fact]
    public void Delete_WhenCalledAgain_ShouldReturnDeletedWithoutMarkingAnythingOrRaisingAnotherEvent()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create(name: "Playlist");
        Result<Deleted> firstResult = playlist.Delete();
        Assert.False(firstResult.IsFailure);
        Assert.Single(playlist.GetDomainEvents());

        // Act
        Result<Deleted> result = playlist.Delete();
        List<IDomainEvent> domainEvents = playlist.GetDomainEvents();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.True(playlist.IsDeleted);
        Assert.Empty(domainEvents);
    }
}
