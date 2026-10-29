#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistCreatedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistCreatedDomainEventTests
{
    private readonly PlaylistIdFixture _playlistIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly PlaylistCreatedDomainEventFixture _playlistCreatedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        PlaylistId playlistId = _playlistIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        PlaylistCreatedDomainEvent domainEvent = _playlistCreatedDomainEventFixture.Create(
            id,
            playlistId,
            userId,
            "Morning",
            PlaylistType.Music,
            occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(playlistId, domainEvent.PlaylistId);
        Assert.Equal(userId, domainEvent.UserId);
        Assert.Equal("Morning", domainEvent.Name);
        Assert.Equal(PlaylistType.Music, domainEvent.Type);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
