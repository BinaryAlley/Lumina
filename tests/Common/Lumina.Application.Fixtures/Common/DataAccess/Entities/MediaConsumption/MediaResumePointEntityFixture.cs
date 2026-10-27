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
/// Fixture class for the <see cref="MediaResumePointEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointEntityFixture
{
    private const int MAXIMUM_OFFSET_MILLISECONDS = 7_200_000;

    private readonly Faker _faker = new();
    private readonly MediaKind[] _resumableMediaKinds =
    [
        MediaKind.MusicTrack,
        MediaKind.PodcastEpisode,
        MediaKind.Audiobook,
        MediaKind.Movie,
        MediaKind.TvEpisode,
        MediaKind.Book,
        MediaKind.Video
    ];

    /// <summary>
    /// Creates a random valid <see cref="MediaResumePointEntity"/>.
    /// </summary>
    /// <remarks>
    /// Every default produced here is accepted by the conversion of the entity to its domain aggregate: the media kind can be resumed, the progress
    /// fraction stays within its boundaries, and a generated position always uses the position kind of its media kind. Supplying contradicting values is
    /// allowed, so that the invalid branches of the conversion can be exercised.
    /// </remarks>
    /// <param name="id">Optional. The Id of the resume point.</param>
    /// <param name="userId">Optional. The Id of the user that owns the resume point.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="mediaKind">Optional. The kind of media that is resumed. Defaults to a random resumable kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the media item to resume.</param>
    /// <param name="lastPositionKind">Optional. The kind of the position at which the media item can be resumed. Defaults to a location for a book, and to a time offset for every other kind.</param>
    /// <param name="lastPositionTimeOffsetMilliseconds">Optional. The position, in milliseconds. Defaults to a bounded random offset for a time offset position, and to <see langword="null"/> otherwise.</param>
    /// <param name="includeLastPositionTimeOffsetMilliseconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="lastPositionLocation">Optional. The opaque locator. Defaults to a random word for a location position, and to <see langword="null"/> otherwise.</param>
    /// <param name="includeLastPositionLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <param name="lastProgressFraction">Optional. The progress fraction at which the media item can be resumed. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="lastConsumedOnUtc">Optional. The date and time when the media item was last consumed. Defaults to a moment in the past.</param>
    /// <param name="version">Optional. The optimistic concurrency version of the row. Defaults to a bounded random positive version.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the row was created. Defaults to the last consumption time.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the row.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the row was last updated. Defaults to no value.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <param name="updatedBy">Optional. The Id of the user that last updated the row. Defaults to <see langword="null"/>.</param>
    /// <param name="includeUpdatedBy">Whether the updating user Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MediaResumePointEntity"/>.</returns>
    public MediaResumePointEntity Create(
        Guid? id = null,
        Guid? userId = null,
        Guid? libraryId = null,
        MediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        MediaPositionKind? lastPositionKind = null,
        long? lastPositionTimeOffsetMilliseconds = null,
        bool includeLastPositionTimeOffsetMilliseconds = true,
        string? lastPositionLocation = null,
        bool includeLastPositionLocation = true,
        double? lastProgressFraction = null,
        DateTime? lastConsumedOnUtc = null,
        long? version = null,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true,
        Guid? updatedBy = null,
        bool includeUpdatedBy = true)
    {
        MediaKind resolvedMediaKind = mediaKind ?? _faker.Random.ArrayElement(_resumableMediaKinds);
        MediaPositionKind resolvedLastPositionKind = lastPositionKind ?? ResolveDefaultPositionKind(resolvedMediaKind);
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
        DateTime resolvedLastConsumedOnUtc = lastConsumedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 720));

        return new MediaResumePointEntity
        {
            Id = id ?? _faker.Random.Guid(),
            UserId = userId ?? _faker.Random.Guid(),
            LibraryId = libraryId ?? _faker.Random.Guid(),
            MediaKind = resolvedMediaKind,
            MediaItemId = mediaItemId ?? _faker.Random.Guid(),
            LastPositionKind = resolvedLastPositionKind,
            LastPositionTimeOffsetMilliseconds = resolvedLastPositionTimeOffsetMilliseconds,
            LastPositionLocation = resolvedLastPositionLocation,
            LastProgressFraction = lastProgressFraction ?? _faker.Random.Double(0, 1),
            LastConsumedOnUtc = resolvedLastConsumedOnUtc,
            Version = version ?? _faker.Random.Long(1, 1000),
            CreatedOnUtc = createdOnUtc ?? resolvedLastConsumedOnUtc,
            CreatedBy = createdBy ?? _faker.Random.Guid(),
            UpdatedOnUtc = includeUpdatedOnUtc ? updatedOnUtc : null,
            UpdatedBy = includeUpdatedBy ? updatedBy : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="MediaResumePointEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaResumePointEntity"/> instances.</returns>
    public List<MediaResumePointEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Resolves the default kind of position used by a media kind.
    /// </summary>
    /// <param name="mediaKind">The kind of media.</param>
    /// <returns>The position kind that a resume point of the media kind uses.</returns>
    private static MediaPositionKind ResolveDefaultPositionKind(MediaKind mediaKind)
    {
        return mediaKind == MediaKind.Book ? MediaPositionKind.Location : MediaPositionKind.TimeOffset;
    }
}
