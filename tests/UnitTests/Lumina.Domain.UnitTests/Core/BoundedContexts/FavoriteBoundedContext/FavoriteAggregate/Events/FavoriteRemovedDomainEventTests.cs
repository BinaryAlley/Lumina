#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="FavoriteRemovedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteRemovedDomainEventTests
{
    private readonly FavoriteIdFixture _favoriteIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly FavoriteRemovedDomainEventFixture _favoriteRemovedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        FavoriteId favoriteId = _favoriteIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        FavoriteMediaKind mediaKind = FavoriteMediaKind.Book;
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        FavoriteRemovedDomainEvent domainEvent = _favoriteRemovedDomainEventFixture.Create(
            id,
            favoriteId,
            userId,
            mediaItemId,
            mediaKind,
            occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(favoriteId, domainEvent.FavoriteId);
        Assert.Equal(userId, domainEvent.UserId);
        Assert.Equal(mediaItemId, domainEvent.MediaItemId);
        Assert.Equal(mediaKind, domainEvent.MediaKind);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
