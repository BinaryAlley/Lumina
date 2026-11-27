#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistDetailsUpdatedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistDetailsUpdatedDomainEventTests
{
    private readonly PlaylistIdFixture _playlistIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly PlaylistDetailsUpdatedDomainEventFixture _playlistDetailsUpdatedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        PlaylistId playlistId = _playlistIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        PlaylistDetailsUpdatedDomainEvent domainEvent = _playlistDetailsUpdatedDomainEventFixture.Create(
            id,
            playlistId,
            userId,
            "Deep Focus",
            occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(playlistId, domainEvent.PlaylistId);
        Assert.Equal(userId, domainEvent.UserId);
        Assert.Equal("Deep Focus", domainEvent.Name);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
