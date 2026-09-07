#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;

/// <summary>
/// Value Object for the rating of an audio media element.
/// </summary>
[DebuggerDisplay("{Value}/{MaxValue}")]
public class AudioRating : Rating
{
    /// <summary>
    /// Gets the optional source of the rating (e.g., "MusicBrainz", "Last.fm").
    /// </summary>
    public Optional<AudioRatingSource> Source { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioRating"/> class.
    /// </summary>
    /// <param name="value">The numeric value of the rating.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="source">The optional source of the rating.</param>
    /// <param name="voteCount">The optional number of votes or reviews.</param>
    private AudioRating(decimal value, decimal maxValue, Optional<AudioRatingSource> source, Optional<int> voteCount)
        : base(value, maxValue, voteCount)
    {
        Source = source;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AudioRating"/> class.
    /// </summary>
    /// <param name="value">The numeric value of the rating.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="source">The optional source of the rating.</param>
    /// <param name="voteCount">The optional number of votes or reviews.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="AudioRating"/>, or an error message.
    /// </returns>
    public static Result<AudioRating> Create(decimal value, decimal maxValue, Optional<AudioRatingSource> source, Optional<int> voteCount)
    {
        if (value > maxValue)
            return Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue;

        return new AudioRating(value, maxValue, source, voteCount);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return MaxValue;
        yield return Source;
        yield return VoteCount;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        string baseString = $"{Value}/{MaxValue}";
        if (Source.HasValue)
            baseString += $" ({Source.Value})";
        if (VoteCount.HasValue)
            baseString += $" [{VoteCount.Value} votes]";
        return baseString;
    }
}
