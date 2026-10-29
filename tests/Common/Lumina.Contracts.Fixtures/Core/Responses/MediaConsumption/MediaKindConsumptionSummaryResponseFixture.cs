#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaKindConsumptionSummaryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaKindConsumptionSummaryResponseFixture
{
    private const double MAXIMUM_DURATION_SECONDS = 36_000;

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

    /// <summary>
    /// Creates a random valid <see cref="MediaKindConsumptionSummaryResponse"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of media the statistics belong to. Defaults to a random supported kind.</param>
    /// <param name="totalConsumedDurationInSeconds">Optional. The total consumed duration of the media kind, in seconds. Defaults to a bounded random duration.</param>
    /// <param name="countedSessionCount">Optional. The number of sessions that count towards statistics.</param>
    /// <param name="completedSessionCount">Optional. The number of sessions that completed.</param>
    /// <param name="stoppedSessionCount">Optional. The number of sessions that stopped before they completed.</param>
    /// <param name="completionRate">Optional. The completion rate. Defaults to a bounded random fraction between zero and one.</param>
    /// <returns>The created <see cref="MediaKindConsumptionSummaryResponse"/>.</returns>
    public MediaKindConsumptionSummaryResponse Create(
        MediaKind? mediaKind = null,
        double? totalConsumedDurationInSeconds = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        int? stoppedSessionCount = null,
        double? completionRate = null)
    {
        return new MediaKindConsumptionSummaryResponse(
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            totalConsumedDurationInSeconds ?? _faker.Random.Double(0, MAXIMUM_DURATION_SECONDS),
            countedSessionCount ?? _faker.Random.Int(0, 100),
            completedSessionCount ?? _faker.Random.Int(0, 100),
            stoppedSessionCount ?? _faker.Random.Int(0, 100),
            completionRate ?? _faker.Random.Double(0, 1));
    }

    /// <summary>
    /// Creates a list of <see cref="MediaKindConsumptionSummaryResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaKindConsumptionSummaryResponse"/> instances.</returns>
    public List<MediaKindConsumptionSummaryResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
