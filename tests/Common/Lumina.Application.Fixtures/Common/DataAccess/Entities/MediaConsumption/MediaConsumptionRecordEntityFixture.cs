#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaConsumptionRecordEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordEntityFixture
{
    private const int MAXIMUM_DURATION_MILLISECONDS = 36_000_000;
    private const int MAXIMUM_OFFSET_MILLISECONDS = 7_200_000;

    private readonly Faker _faker = new();
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
    /// Creates a random valid <see cref="MediaConsumptionRecordEntity"/>.
    /// </summary>
    /// <remarks>
    /// Every default produced here is accepted by the conversion of the entity to its domain aggregate: the status and end time agree, the last
    /// activity never precedes the start, the end never precedes the last activity, consecutive pages are only counted for a book, and a generated
    /// position always uses the position kind of its media kind. Supplying contradicting values is allowed, so that the invalid branches of the
    /// conversion can be exercised.
    /// </remarks>
    /// <param name="id">Optional. The Id of the consumption record.</param>
    /// <param name="userId">Optional. The Id of the user that started the session.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="mediaKind">Optional. The kind of media that was consumed. Defaults to a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the consumed media item.</param>
    /// <param name="status">Optional. The state of the session. Defaults to a random valid status.</param>
    /// <param name="startedOnUtc">Optional. The date and time when the session started. Defaults to a moment in the past, or before the last activity when it is supplied.</param>
    /// <param name="lastActivityOnUtc">Optional. The date and time when the session last received activity. Defaults to a moment at or after the start.</param>
    /// <param name="endedOnUtc">Optional. The date and time when the session ended. Defaults to no value for an in-progress session, and to a moment at or after the last activity for a terminal session.</param>
    /// <param name="consumedDurationMilliseconds">Optional. The active duration consumed by the session, in milliseconds. Defaults to a bounded random duration.</param>
    /// <param name="lastPositionKind">Optional. The kind of the last known position. Defaults to no position for a photo, a location for a book, and a time offset for every other kind.</param>
    /// <param name="lastPositionTimeOffsetMilliseconds">Optional. The last known position, in milliseconds. Defaults to a bounded random offset for a time offset position, and to <see langword="null"/> otherwise.</param>
    /// <param name="lastPositionLocation">Optional. The last known opaque locator. Defaults to a random word for a location position, and to <see langword="null"/> otherwise.</param>
    /// <param name="lastProgressFraction">Optional. The latest reported progress fraction. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="consecutivePagesRead">Optional. The number of consecutive pages read. Defaults to a bounded random count for a book, and to zero for every other kind.</param>
    /// <param name="version">Optional. The optimistic concurrency version of the row. Defaults to a bounded random positive version.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the row was created. Defaults to the start of the session.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the row.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the row was last updated. Defaults to no value.</param>
    /// <param name="updatedBy">Optional. The Id of the user that last updated the row. Defaults to <see langword="null"/>.</param>
    /// <param name="includeEndedOnUtc">Whether the end time should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLastPositionKind">Whether the position kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLastPositionTimeOffsetMilliseconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLastPositionLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedBy">Whether the updating user Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MediaConsumptionRecordEntity"/>.</returns>
    public MediaConsumptionRecordEntity Create(
        Guid? id = null,
        Guid? userId = null,
        Guid? libraryId = null,
        MediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        ConsumptionStatus? status = null,
        DateTime? startedOnUtc = null,
        DateTime? lastActivityOnUtc = null,
        DateTime? endedOnUtc = null,
        long? consumedDurationMilliseconds = null,
        MediaPositionKind? lastPositionKind = null,
        long? lastPositionTimeOffsetMilliseconds = null,
        string? lastPositionLocation = null,
        double? lastProgressFraction = null,
        int? consecutivePagesRead = null,
        long? version = null,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null,
        DateTime? updatedOnUtc = null,
        Guid? updatedBy = null,
        bool includeEndedOnUtc = true,
        bool includeLastPositionKind = true,
        bool includeLastPositionTimeOffsetMilliseconds = true,
        bool includeLastPositionLocation = true,
        bool includeUpdatedOnUtc = true,
        bool includeUpdatedBy = true)
    {
        MediaKind resolvedMediaKind = mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds);
        ConsumptionStatus resolvedStatus = status ?? _faker.Random.ArrayElement(_statuses);
        DateTime resolvedStartedOnUtc = startedOnUtc ?? (lastActivityOnUtc.HasValue
            ? lastActivityOnUtc.Value.AddMinutes(-_faker.Random.Int(0, 120))
            : DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).AddMinutes(-_faker.Random.Int(0, 59)));
        DateTime resolvedLastActivityOnUtc = lastActivityOnUtc ?? resolvedStartedOnUtc.AddMinutes(_faker.Random.Int(0, 120));
        DateTime? resolvedEndedOnUtc = includeEndedOnUtc
            ? endedOnUtc ?? (resolvedStatus == ConsumptionStatus.InProgress
                ? null
                : resolvedLastActivityOnUtc.AddMinutes(_faker.Random.Int(0, 60)))
            : null;
        MediaPositionKind? resolvedLastPositionKind = includeLastPositionKind
            ? lastPositionKind ?? ResolveDefaultPositionKind(resolvedMediaKind)
            : null;
        long? resolvedLastPositionTimeOffsetMilliseconds = includeLastPositionTimeOffsetMilliseconds
            ? lastPositionTimeOffsetMilliseconds ?? (resolvedLastPositionKind == MediaPositionKind.TimeOffset
                ? _faker.Random.Long(0, MAXIMUM_OFFSET_MILLISECONDS)
                : (long?)null)
            : null;
        string? resolvedLastPositionLocation = includeLastPositionLocation
            ? lastPositionLocation ?? (resolvedLastPositionKind == MediaPositionKind.Location
                ? _faker.Lorem.Word()
                : (string?)null)
            : null;

        return new MediaConsumptionRecordEntity
        {
            Id = id ?? _faker.Random.Guid(),
            UserId = userId ?? _faker.Random.Guid(),
            LibraryId = libraryId ?? _faker.Random.Guid(),
            MediaKind = resolvedMediaKind,
            MediaItemId = mediaItemId ?? _faker.Random.Guid(),
            Status = resolvedStatus,
            StartedOnUtc = resolvedStartedOnUtc,
            LastActivityOnUtc = resolvedLastActivityOnUtc,
            EndedOnUtc = resolvedEndedOnUtc,
            ConsumedDurationMilliseconds = consumedDurationMilliseconds ?? _faker.Random.Long(0, MAXIMUM_DURATION_MILLISECONDS),
            LastPositionKind = resolvedLastPositionKind,
            LastPositionTimeOffsetMilliseconds = resolvedLastPositionTimeOffsetMilliseconds,
            LastPositionLocation = resolvedLastPositionLocation,
            LastProgressFraction = lastProgressFraction ?? _faker.Random.Double(0, 1),
            ConsecutivePagesRead = consecutivePagesRead ?? (resolvedMediaKind == MediaKind.Book ? _faker.Random.Int(0, 100) : 0),
            Version = version ?? _faker.Random.Long(1, 1000),
            CreatedOnUtc = createdOnUtc ?? resolvedStartedOnUtc,
            CreatedBy = createdBy ?? _faker.Random.Guid(),
            UpdatedOnUtc = includeUpdatedOnUtc ? updatedOnUtc : null,
            UpdatedBy = includeUpdatedBy ? updatedBy : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="MediaConsumptionRecordEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaConsumptionRecordEntity"/> instances.</returns>
    public List<MediaConsumptionRecordEntity> CreateMany(int count = 3)
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
