#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;

/// <summary>
/// Fixture class for the <see cref="MediaConsumptionRecord"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordFixture
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
    /// Creates a random valid <see cref="MediaConsumptionRecord"/> domain aggregate, with pre-existing data.
    /// </summary>
    /// <param name="id">Optional. The Id of the consumption record.</param>
    /// <param name="userId">Optional. The Id of the user that owns the consumption record.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the consumed media item.</param>
    /// <param name="mediaItemId">Optional. The Id of the consumed media item.</param>
    /// <param name="mediaKind">Optional. The kind of media that is consumed. Defaults to a random supported kind.</param>
    /// <param name="status">Optional. The status of the consumption session. Defaults to a random valid status.</param>
    /// <param name="startedOnUtc">Optional. The date and time when the consumption session started. Defaults to a few days in the past.</param>
    /// <param name="lastActivityOnUtc">Optional. The date and time of the last activity of the consumption session. Defaults to a moment at or after the start.</param>
    /// <param name="endedOnUtc">Optional. The date and time when the consumption session ended. Defaults to no value for an in-progress session, and to a moment at or after the last activity for a terminal session.</param>
    /// <param name="consumedDuration">Optional. The active duration consumed by the session. Defaults to a bounded random duration.</param>
    /// <param name="lastPosition">Optional. The last position of the session. When omitted, a position matching the position kind of the media kind rule is generated.</param>
    /// <param name="includeLastPosition">Whether to generate a last position when <paramref name="lastPosition"/> is not supplied. Must be <see langword="false"/> for a photo, which has no position kind and accepts no position at all.</param>
    /// <param name="lastProgressFraction">Optional. The last progress fraction of the session. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="consecutivePagesRead">Optional. The number of consecutive pages read. Defaults to a bounded random count for a page based kind, and to zero for every other kind.</param>
    /// <returns>The created <see cref="MediaConsumptionRecord"/>.</returns>
    /// <remarks>
    /// The record is built through <see cref="MediaConsumptionRecord.Reconstitute"/>, so every default produced here is internally consistent: the
    /// start is in the past, the last activity never precedes it, an end time exists exactly when the status is terminal and never precedes the last
    /// activity, consecutive pages are only counted for a book, and a generated position always uses the position kind of its media kind.
    /// </remarks>
    public MediaConsumptionRecord Create(
        MediaConsumptionRecordId? id = null,
        UserId? userId = null,
        LibraryId? libraryId = null,
        MediaItemId? mediaItemId = null,
        MediaKind? mediaKind = null,
        ConsumptionStatus? status = null,
        DateTime? startedOnUtc = null,
        DateTime? lastActivityOnUtc = null,
        Optional<DateTime>? endedOnUtc = null,
        TimeSpan? consumedDuration = null,
        Optional<MediaPosition>? lastPosition = null,
        bool includeLastPosition = true,
        double? lastProgressFraction = null,
        int? consecutivePagesRead = null)
    {
        MediaKind resolvedMediaKind = mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds);
        ConsumptionStatus resolvedStatus = status ?? _faker.Random.ArrayElement([ConsumptionStatus.InProgress, ConsumptionStatus.Completed, ConsumptionStatus.Stopped]);
        DateTime resolvedStartedOnUtc = startedOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).AddMinutes(-_faker.Random.Int(0, 59));
        DateTime resolvedLastActivityOnUtc = lastActivityOnUtc ?? resolvedStartedOnUtc.AddMinutes(_faker.Random.Int(0, 120));
        Optional<DateTime> resolvedEndedOnUtc = endedOnUtc ?? (resolvedStatus == ConsumptionStatus.InProgress
            ? Optional<DateTime>.None()
            : Optional<DateTime>.Some(resolvedLastActivityOnUtc == DateTime.MaxValue
                ? DateTime.MaxValue
                : resolvedLastActivityOnUtc.AddMinutes(_faker.Random.Int(0, 60))));
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(resolvedMediaKind);
        bool isPageBased = ruleResult.HasValue && ruleResult.Value is PageThresholdConsumptionRule;

        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            id ?? _mediaConsumptionRecordIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            resolvedMediaKind,
            resolvedStatus,
            resolvedStartedOnUtc,
            resolvedLastActivityOnUtc,
            resolvedEndedOnUtc,
            consumedDuration ?? TimeSpan.FromSeconds(_faker.Random.Int(0, 36000)),
            ResolveLastPosition(lastPosition, includeLastPosition, ruleResult),
            lastProgressFraction ?? _faker.Random.Double(0, 1),
            consecutivePagesRead ?? (isPageBased ? _faker.Random.Int(0, 100) : 0));
        return result.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MediaConsumptionRecord"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaConsumptionRecord"/> instances.</returns>
    public List<MediaConsumptionRecord> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Resolves the last position of the record, keeping it consistent with the position kind of its media kind rule.
    /// </summary>
    /// <param name="lastPosition">The explicitly supplied last position, if any.</param>
    /// <param name="includeLastPosition">Whether to generate a position when none is supplied.</param>
    /// <param name="ruleResult">The consumption rule of the media kind of the record, if the kind has one.</param>
    /// <returns>The resolved last position, or no value when the record carries no position.</returns>
    private Optional<MediaPosition> ResolveLastPosition(Optional<MediaPosition>? lastPosition, bool includeLastPosition, Optional<MediaConsumptionRule> ruleResult)
    {
        if (!includeLastPosition)
            return Optional<MediaPosition>.None();
        if (lastPosition.HasValue)
            return lastPosition.Value;
        if (!ruleResult.HasValue || !ruleResult.Value.PositionKind.HasValue)
            return Optional<MediaPosition>.None();
        return Optional<MediaPosition>.Some(_mediaPositionFixture.Create(kind: ruleResult.Value.PositionKind.Value));
    }
}
