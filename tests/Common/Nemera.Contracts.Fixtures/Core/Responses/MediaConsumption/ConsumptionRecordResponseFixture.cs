#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ConsumptionRecordResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionRecordResponseFixture
{
    private const double MAXIMUM_DURATION_SECONDS = 36_000;

    private readonly Faker _faker = new();
    private readonly MediaPositionResponseFixture _mediaPositionResponseFixture = new();
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
    private readonly ConsumptionStatus[] _statuses =
    [
        ConsumptionStatus.InProgress,
        ConsumptionStatus.Completed,
        ConsumptionStatus.Stopped
    ];

    /// <summary>
    /// Creates a random valid <see cref="ConsumptionRecordResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the consumption record.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the consumed media item.</param>
    /// <param name="mediaItemId">Optional. The Id of the consumed media item.</param>
    /// <param name="mediaKind">Optional. The kind of media that was consumed. Defaults to a random supported kind.</param>
    /// <param name="status">Optional. The state of the consumption session. Defaults to a random valid status.</param>
    /// <param name="startedOnUtc">Optional. The date and time when the session started. Defaults to a moment in the past.</param>
    /// <param name="lastActivityOnUtc">Optional. The date and time when the session last received activity. Defaults to a moment at or after the start.</param>
    /// <param name="endedOnUtc">Optional. The date and time when the session ended. Defaults to no value for an in-progress session, and to a moment at or after the last activity for a terminal session.</param>
    /// <param name="includeEndedOnUtc">Whether the end time should be included, or forced to <see langword="null"/>.</param>
    /// <param name="consumedDurationInSeconds">Optional. The active duration consumed by the session, in seconds. Defaults to a bounded random duration.</param>
    /// <param name="lastPosition">Optional. The last known position. Defaults to a position matching the position kind of the media kind, and to no position for a photo.</param>
    /// <param name="includeLastPosition">Whether the last position should be included, or forced to <see langword="null"/>.</param>
    /// <param name="lastProgressFraction">Optional. The latest reported progress fraction. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="consecutivePagesRead">Optional. The number of consecutive pages read. Defaults to a bounded random count for a book, and to zero for every other kind.</param>
    /// <returns>The created <see cref="ConsumptionRecordResponse"/>.</returns>
    public ConsumptionRecordResponse Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? mediaItemId = null,
        MediaKind? mediaKind = null,
        ConsumptionStatus? status = null,
        DateTime? startedOnUtc = null,
        DateTime? lastActivityOnUtc = null,
        DateTime? endedOnUtc = null,
        bool includeEndedOnUtc = true,
        double? consumedDurationInSeconds = null,
        MediaPositionResponse? lastPosition = null,
        bool includeLastPosition = true,
        double? lastProgressFraction = null,
        int? consecutivePagesRead = null)
    {
        MediaKind resolvedMediaKind = mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds);
        ConsumptionStatus resolvedStatus = status ?? _faker.Random.ArrayElement(_statuses);
        DateTime resolvedStartedOnUtc = startedOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).AddMinutes(-_faker.Random.Int(0, 59));
        DateTime resolvedLastActivityOnUtc = lastActivityOnUtc ?? resolvedStartedOnUtc.AddMinutes(_faker.Random.Int(0, 120));
        DateTime? resolvedEndedOnUtc = includeEndedOnUtc
            ? endedOnUtc ?? (resolvedStatus == ConsumptionStatus.InProgress
                ? null
                : resolvedLastActivityOnUtc.AddMinutes(_faker.Random.Int(0, 60)))
            : null;
        MediaPositionKind? defaultPositionKind = ResolveDefaultPositionKind(resolvedMediaKind);
        MediaPositionResponse? resolvedLastPosition = includeLastPosition && (lastPosition is not null || defaultPositionKind.HasValue)
            ? lastPosition ?? _mediaPositionResponseFixture.Create(kind: defaultPositionKind)
            : null;

        return new ConsumptionRecordResponse(
            id ?? _faker.Random.Guid(),
            libraryId ?? _faker.Random.Guid(),
            mediaItemId ?? _faker.Random.Guid(),
            resolvedMediaKind,
            resolvedStatus,
            resolvedStartedOnUtc,
            resolvedLastActivityOnUtc,
            resolvedEndedOnUtc,
            consumedDurationInSeconds ?? _faker.Random.Double(0, MAXIMUM_DURATION_SECONDS),
            resolvedLastPosition,
            lastProgressFraction ?? _faker.Random.Double(0, 1),
            consecutivePagesRead ?? (resolvedMediaKind == MediaKind.Book ? _faker.Random.Int(0, 100) : 0));
    }

    /// <summary>
    /// Creates a list of <see cref="ConsumptionRecordResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumptionRecordResponse"/> instances.</returns>
    public List<ConsumptionRecordResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Resolves the default kind of position used by a media kind.
    /// </summary>
    /// <param name="mediaKind">The kind of media.</param>
    /// <returns>The position kind that a session of the media kind reports, or <see langword="null"/> when the kind carries no position.</returns>
    private static MediaPositionKind? ResolveDefaultPositionKind(MediaKind mediaKind)
    {
        if (mediaKind == MediaKind.Photo)
            return null;
        return mediaKind == MediaKind.Book ? MediaPositionKind.Location : MediaPositionKind.TimeOffset;
    }
}
