#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="MediaConsumptionStartedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionStartedDomainEventFixture
{
    private readonly Faker _faker = new();
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaKind[] _supportedMediaKinds =
    [
        MediaKind.MusicTrack,
        MediaKind.PodcastEpisode,
        MediaKind.Audiobook,
        MediaKind.Movie,
        MediaKind.TvEpisode,
        MediaKind.Book,
        MediaKind.Photo,
        MediaKind.Video
    ];

    /// <summary>
    /// Creates a random valid <see cref="MediaConsumptionStartedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="mediaConsumptionRecordId">Optional. The unique identifier of the consumption record.</param>
    /// <param name="userId">Optional. The unique identifier of the user that started the session.</param>
    /// <param name="libraryId">Optional. The unique identifier of the media library that contains the consumed item.</param>
    /// <param name="mediaItemId">Optional. The unique identifier of the consumed media item.</param>
    /// <param name="mediaKind">Optional. The kind of media that is being consumed. Defaults to a random supported kind.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="MediaConsumptionStartedDomainEvent"/>.</returns>
    public MediaConsumptionStartedDomainEvent Create(
        Guid? id = null,
        MediaConsumptionRecordId? mediaConsumptionRecordId = null,
        UserId? userId = null,
        LibraryId? libraryId = null,
        MediaItemId? mediaItemId = null,
        MediaKind? mediaKind = null,
        DateTime? occurredOnUtc = null)
    {
        return new MediaConsumptionStartedDomainEvent(
            id ?? Guid.NewGuid(),
            mediaConsumptionRecordId ?? _mediaConsumptionRecordIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
