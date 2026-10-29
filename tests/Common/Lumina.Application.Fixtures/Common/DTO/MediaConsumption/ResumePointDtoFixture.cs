#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ResumePointDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ResumePointDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();
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
    /// Creates a random valid <see cref="ResumePointDto"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the resume point.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="mediaItemId">Optional. The Id of the media item to resume.</param>
    /// <param name="mediaKind">Optional. The kind of media that is resumed. Defaults to a random resumable kind.</param>
    /// <param name="lastPosition">Optional. The position at which the media item can be resumed. Defaults to a position matching the position kind of the media kind.</param>
    /// <param name="lastProgressFraction">Optional. The progress fraction at which the media item can be resumed. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="lastConsumedOnUtc">Optional. The date and time when the media item was last consumed. Defaults to a moment in the past.</param>
    /// <returns>The created <see cref="ResumePointDto"/>.</returns>
    public ResumePointDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? mediaItemId = null,
        MediaKind? mediaKind = null,
        MediaPositionDto? lastPosition = null,
        double? lastProgressFraction = null,
        DateTime? lastConsumedOnUtc = null)
    {
        MediaKind resolvedMediaKind = mediaKind ?? _faker.Random.ArrayElement(_resumableMediaKinds);

        return new ResumePointDto(
            id ?? _faker.Random.Guid(),
            libraryId ?? _faker.Random.Guid(),
            mediaItemId ?? _faker.Random.Guid(),
            resolvedMediaKind,
            lastPosition ?? _mediaPositionDtoFixture.Create(kind: ResolveDefaultPositionKind(resolvedMediaKind)),
            lastProgressFraction ?? _faker.Random.Double(0, 1),
            lastConsumedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 720)));
    }

    /// <summary>
    /// Creates a list of <see cref="ResumePointDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ResumePointDto"/> instances.</returns>
    public List<ResumePointDto> CreateMany(int count = 3)
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
