#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Playlist"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistTests
{
    [Fact]
    public void Delete_WhenCalled_ShouldMarkThePlaylistAsDeletedAndRaiseOneEvent()
    {
        // Arrange
        Result<Playlist> createResult = Playlist.Create(
            UserId.Create(Guid.NewGuid()),
            "Playlist",
            Optional<string>.None(),
            PlaylistType.Music);
        Assert.False(createResult.IsFailure);
        Playlist playlist = createResult.Value;
        Assert.False(playlist.IsDeleted);

        // Act
        Result<Deleted> result = playlist.Delete();
        List<IDomainEvent> domainEvents = playlist.GetDomainEvents();

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(playlist.IsDeleted);
        Assert.Single(domainEvents);
        Assert.IsType<PlaylistDeletedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void Delete_WhenCalledAgain_ShouldNotRaiseAnotherEvent()
    {
        // Arrange
        Result<Playlist> createResult = Playlist.Create(
            UserId.Create(Guid.NewGuid()),
            "Playlist",
            Optional<string>.None(),
            PlaylistType.Music);
        Assert.False(createResult.IsFailure);
        Playlist playlist = createResult.Value;
        playlist.Delete();

        // Act
        Result<Deleted> result = playlist.Delete();
        List<IDomainEvent> domainEvents = playlist.GetDomainEvents();

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(playlist.IsDeleted);
        Assert.Single(domainEvents);
    }
}
