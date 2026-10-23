#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Value Object for the consumption rules of a kind of media.
/// </summary>
/// <remarks>
/// The rules are shared by the consumption record aggregate, the resume point aggregate and the statistics read model, so they live in the common
/// folder of the bounded context. Each concrete rule describes the counting behavior and the resumability of one media kind, and the factory returns
/// the rule for a kind so that callers never switch on the media kind themselves.
/// </remarks>
public abstract class MediaConsumptionRule : ValueObject
{
    /// <summary>
    /// Gets the kind of media that this rule applies to.
    /// </summary>
    public abstract MediaKind MediaKind { get; }

    /// <summary>
    /// Gets a value indicating whether the current catalogues support this kind of media.
    /// </summary>
    public abstract bool IsSupportedByCurrentCatalogues { get; }

    /// <summary>
    /// Gets the kind of position used to resume this kind of media, if the kind can be resumed.
    /// </summary>
    public abstract Optional<MediaPositionKind> PositionKind { get; }

    /// <summary>
    /// Gets a value indicating whether a consumption session of this kind of media can be resumed.
    /// </summary>
    public bool CanResume => PositionKind.HasValue;

    /// <summary>
    /// Determines whether a consumption session of this kind of media counts towards statistics.
    /// </summary>
    /// <param name="status">The status of the consumption session.</param>
    /// <param name="consumedDuration">The active duration consumed by the consumption session.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the consumption session.</param>
    /// <returns><see langword="true"/> if the consumption session counts towards statistics, <see langword="false"/> otherwise.</returns>
    public abstract bool Counts(ConsumptionStatus status, TimeSpan consumedDuration, int consecutivePagesRead);

    /// <summary>
    /// Gets the consumption rule for <paramref name="mediaKind"/>.
    /// </summary>
    /// <param name="mediaKind">The kind of media for which to get the consumption rule.</param>
    /// <returns>
    /// An <see cref="Optional{TValue}"/> containing the consumption rule for <paramref name="mediaKind"/>, or no value when the kind has no rule.
    /// </returns>
    public static Optional<MediaConsumptionRule> For(MediaKind mediaKind)
    {
        return mediaKind switch
        {
            MediaKind.MusicTrack => Optional<MediaConsumptionRule>.Some(new MusicTrackConsumptionRule()),
            MediaKind.PodcastEpisode => Optional<MediaConsumptionRule>.Some(new PodcastEpisodeConsumptionRule()),
            MediaKind.Audiobook => Optional<MediaConsumptionRule>.Some(new AudiobookConsumptionRule()),
            MediaKind.Movie => Optional<MediaConsumptionRule>.Some(new MovieConsumptionRule()),
            MediaKind.TvEpisode => Optional<MediaConsumptionRule>.Some(new TvEpisodeConsumptionRule()),
            MediaKind.Book => Optional<MediaConsumptionRule>.Some(new BookConsumptionRule()),
            MediaKind.Photo => Optional<MediaConsumptionRule>.Some(new PhotoConsumptionRule()),
            MediaKind.Video => Optional<MediaConsumptionRule>.Some(new VideoConsumptionRule()),
            _ => Optional<MediaConsumptionRule>.None()
        };
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MediaKind;
    }
}
