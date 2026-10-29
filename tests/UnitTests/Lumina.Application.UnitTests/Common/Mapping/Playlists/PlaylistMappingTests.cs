#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistMappingTests
{
    private readonly PlaylistFixture _playlistFixture = new();
    private readonly PlaylistEntryFixture _playlistEntryFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingTheAggregate_ShouldMapEveryScalar()
    {
        // Arrange
        DateTime createdOnUtc = new(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 4, 5, 6, 7, 8, DateTimeKind.Utc);
        Playlist playlist = _playlistFixture.Create(
            name: "Focus",
            description: "Deep work",
            type: PlaylistType.Music,
            createdOnUtc: createdOnUtc,
            updatedOnUtc: updatedOnUtc,
            includeUpdatedOnUtc: true);

        // Act
        PlaylistEntity result = playlist.ToRepositoryEntity();

        // Assert
        Assert.Equal(playlist.Id.Value, result.Id);
        Assert.Equal(playlist.UserId.Value, result.UserId);
        Assert.Equal("Focus", result.Name);
        Assert.Equal("Deep work", result.Description);
        Assert.Equal(PlaylistType.Music, result.Type);
        Assert.Equal(createdOnUtc, result.CreatedOnUtc);
        Assert.Equal(updatedOnUtc, result.UpdatedOnUtc);
        Assert.Equal(Guid.Empty, result.CreatedBy);
        Assert.Null(result.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenTheDescriptionAndUpdatedOnUtcAreMissing_ShouldMapNulls()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create(
            includeDescription: false,
            includeUpdatedOnUtc: false);

        // Act
        PlaylistEntity result = playlist.ToRepositoryEntity();

        // Assert
        Assert.Null(result.Description);
        Assert.Null(result.UpdatedOnUtc);
    }

    [Fact]
    public void ToRepositoryEntity_WhenTheAggregateHasEntries_ShouldMapThemOrderedByPositionAndCountThem()
    {
        // Arrange
        PlaylistEntry lastEntry = _playlistEntryFixture.Create(position: 900);
        PlaylistEntry firstEntry = _playlistEntryFixture.Create(position: 100);
        List<PlaylistEntry> entries = [lastEntry, firstEntry];
        Playlist playlist = _playlistFixture.Create(entries: entries);

        // Act
        PlaylistEntity result = playlist.ToRepositoryEntity();

        // Assert
        Assert.Equal(2, result.EntryCount);
        Assert.Equal([100L, 900L], result.Entries.Select(entry => entry.Position));
        Assert.Equal([firstEntry.Id.Value, lastEntry.Id.Value], result.Entries.Select(entry => entry.Id));
        Assert.All(result.Entries, entry => Assert.Equal(playlist.Id.Value, entry.PlaylistId));
    }

    [Fact]
    public void ToRepositoryEntity_WhenTheAggregateHasNoEntries_ShouldMapAnEmptyCollectionAndAZeroCount()
    {
        // Arrange
        Playlist playlist = _playlistFixture.Create();

        // Act
        PlaylistEntity result = playlist.ToRepositoryEntity();

        // Assert
        Assert.Empty(result.Entries);
        Assert.Equal(0, result.EntryCount);
    }
}
