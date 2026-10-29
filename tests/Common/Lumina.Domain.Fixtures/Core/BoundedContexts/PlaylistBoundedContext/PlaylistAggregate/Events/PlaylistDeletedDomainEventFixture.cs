#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="PlaylistDeletedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistDeletedDomainEventFixture
{
    private readonly PlaylistIdFixture _playlistIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistDeletedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="playlistId">Optional. The unique identifier of the playlist.</param>
    /// <param name="userId">Optional. The unique identifier of the user that owned the playlist.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="PlaylistDeletedDomainEvent"/>.</returns>
    public PlaylistDeletedDomainEvent Create(
        Guid? id = null,
        PlaylistId? playlistId = null,
        UserId? userId = null,
        DateTime? occurredOnUtc = null)
    {
        return new PlaylistDeletedDomainEvent(
            id ?? Guid.NewGuid(),
            playlistId ?? _playlistIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistDeletedDomainEvent"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistDeletedDomainEvent"/> instances.</returns>
    public List<PlaylistDeletedDomainEvent> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
