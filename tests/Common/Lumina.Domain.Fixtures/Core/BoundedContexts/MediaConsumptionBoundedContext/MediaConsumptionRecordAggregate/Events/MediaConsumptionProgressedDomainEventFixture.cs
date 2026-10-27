#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
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
/// Fixture class for the <see cref="MediaConsumptionProgressedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionProgressedDomainEventFixture
{
    private readonly Faker _faker = new();
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();
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
    /// Creates a random valid <see cref="MediaConsumptionProgressedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="mediaConsumptionRecordId">Optional. The unique identifier of the consumption record.</param>
    /// <param name="userId">Optional. The unique identifier of the user that owns the consumption record.</param>
    /// <param name="libraryId">Optional. The unique identifier of the media library that contains the consumed item.</param>
    /// <param name="mediaItemId">Optional. The unique identifier of the consumed media item.</param>
    /// <param name="mediaKind">Optional. The kind of media that is being consumed. Defaults to a random supported kind.</param>
    /// <param name="lastPosition">Optional. The last known position for the media item. Defaults to no position, unless <paramref name="includeLastPosition"/> is set.</param>
    /// <param name="includeLastPosition">Whether to generate a last position when none is supplied. Must be <see langword="false"/> for a photo, which has no position kind and accepts no position at all.</param>
    /// <param name="lastProgressFraction">Optional. The last progress fraction reported by the client. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="consumedDuration">Optional. The active duration consumed so far. Defaults to a bounded random duration.</param>
    /// <param name="consecutivePagesRead">Optional. The number of consecutive pages read so far. Defaults to a bounded random count for a page based kind, and to zero for every other kind.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="MediaConsumptionProgressedDomainEvent"/>.</returns>
    public MediaConsumptionProgressedDomainEvent Create(
        Guid? id = null,
        MediaConsumptionRecordId? mediaConsumptionRecordId = null,
        UserId? userId = null,
        LibraryId? libraryId = null,
        MediaItemId? mediaItemId = null,
        MediaKind? mediaKind = null,
        Optional<MediaPosition>? lastPosition = null,
        bool includeLastPosition = false,
        double? lastProgressFraction = null,
        TimeSpan? consumedDuration = null,
        int? consecutivePagesRead = null,
        DateTime? occurredOnUtc = null)
    {
        MediaKind resolvedMediaKind = mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds);
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(resolvedMediaKind);
        bool isPageBased = ruleResult.HasValue && ruleResult.Value is PageThresholdConsumptionRule;

        return new MediaConsumptionProgressedDomainEvent(
            id ?? Guid.NewGuid(),
            mediaConsumptionRecordId ?? _mediaConsumptionRecordIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            resolvedMediaKind,
            lastPosition ?? ResolveLastPosition(includeLastPosition, ruleResult),
            lastProgressFraction ?? _faker.Random.Double(0, 1),
            consumedDuration ?? TimeSpan.FromSeconds(_faker.Random.Int(0, 36000)),
            consecutivePagesRead ?? (isPageBased ? _faker.Random.Int(0, 100) : 0),
            occurredOnUtc ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Resolves the last position of the domain event, keeping it consistent with the position kind of the media kind rule.
    /// </summary>
    /// <param name="includeLastPosition">Whether to generate a position.</param>
    /// <param name="ruleResult">The consumption rule of the media kind of the domain event, if the kind has one.</param>
    /// <returns>The resolved last position, or no value when the domain event carries no position.</returns>
    private Optional<MediaPosition> ResolveLastPosition(bool includeLastPosition, Optional<MediaConsumptionRule> ruleResult)
    {
        if (!includeLastPosition)
            return Optional<MediaPosition>.None();
        if (!ruleResult.HasValue || !ruleResult.Value.PositionKind.HasValue)
            return Optional<MediaPosition>.None();
        return Optional<MediaPosition>.Some(_mediaPositionFixture.Create(kind: ruleResult.Value.PositionKind.Value));
    }
}
