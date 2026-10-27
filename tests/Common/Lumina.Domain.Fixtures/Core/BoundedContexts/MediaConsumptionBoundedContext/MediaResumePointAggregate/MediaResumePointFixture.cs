#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;

/// <summary>
/// Fixture class for the <see cref="MediaResumePoint"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointFixture
{
    private readonly Faker _faker = new();
    private readonly MediaResumePointIdFixture _mediaResumePointIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();
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
    /// Creates a random valid <see cref="MediaResumePoint"/> domain aggregate, with pre-existing data.
    /// </summary>
    /// <param name="id">Optional. The Id of the resume point.</param>
    /// <param name="userId">Optional. The Id of the user that owns the resume point.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="mediaItemId">Optional. The Id of the media item to resume.</param>
    /// <param name="mediaKind">Optional. The kind of media that is resumed. Defaults to a random resumable kind.</param>
    /// <param name="lastPosition">Optional. The position at which the media item can be resumed. When omitted, a position matching the position kind of the media kind rule is generated.</param>
    /// <param name="lastProgressFraction">Optional. The progress fraction at which the media item can be resumed. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="lastConsumedOnUtc">Optional. The date and time when the media item was last consumed. Defaults to a moment in the past.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the resume point was created. Defaults to the resolved last consumption time.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the resume point was last updated. Defaults to no value.</param>
    /// <returns>The created <see cref="MediaResumePoint"/>.</returns>
    /// <remarks>
    /// The resume point is built through <see cref="MediaResumePoint.Reconstitute"/>, so every default produced here is internally consistent: the
    /// media kind is always one that can be resumed, a generated position always uses the position kind of its media kind rule, the progress fraction
    /// stays within its boundaries, the last consumption time is in the past and the creation time defaults to it. The fixture must never be used with a
    /// non-resumable kind, such as <see cref="MediaKind.Photo"/>, or with an unsupported kind, because no valid resume point exists for them. A resume
    /// point always carries a position, so the fixture generates one whenever <paramref name="lastPosition"/> is omitted.
    /// </remarks>
    public MediaResumePoint Create(
        MediaResumePointId? id = null,
        UserId? userId = null,
        LibraryId? libraryId = null,
        MediaItemId? mediaItemId = null,
        MediaKind? mediaKind = null,
        MediaPosition? lastPosition = null,
        double? lastProgressFraction = null,
        DateTime? lastConsumedOnUtc = null,
        DateTime? createdOnUtc = null,
        Optional<DateTime>? updatedOnUtc = null)
    {
        MediaKind resolvedMediaKind = mediaKind ?? _faker.Random.ArrayElement(_resumableMediaKinds);
        MediaConsumptionRule rule = MediaConsumptionRule.For(resolvedMediaKind).Value;
        DateTime resolvedLastConsumedOnUtc = lastConsumedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 720));
        MediaPosition resolvedLastPosition = lastPosition ?? _mediaPositionFixture.Create(kind: rule.PositionKind.Value);

        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            id ?? _mediaResumePointIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            resolvedMediaKind,
            resolvedLastPosition,
            lastProgressFraction ?? _faker.Random.Double(0, 1),
            resolvedLastConsumedOnUtc,
            createdOnUtc ?? resolvedLastConsumedOnUtc,
            updatedOnUtc ?? Optional<DateTime>.None());
        return result.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MediaResumePoint"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaResumePoint"/> instances.</returns>
    public List<MediaResumePoint> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
