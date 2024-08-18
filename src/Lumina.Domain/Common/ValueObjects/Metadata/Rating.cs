#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the rating of a media element.
/// </summary>
[DebuggerDisplay("{Value}/{MaxValue}")]
public class Rating : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the numeric value of the rating.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets the maximum possible rating value.
    /// </summary>
    public decimal MaxValue { get; }

    /// <summary>
    /// Gets the optional source of the rating (e.g., "IMDB", "Rotten Tomatoes").
    /// </summary>
    public Optional<RatingSource> Source { get; }

    /// <summary>
    /// Gets the optional number of votes or reviews this rating is based on.
    /// </summary>
    public Optional<int> VoteCount { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="value">The numeric value of the rating.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="source">The optional source of the rating.</param>
    /// <param name="voteCount">The optional number of votes or reviews.</param>
    private Rating(decimal value, decimal maxValue, Optional<RatingSource> source = default, Optional<int> voteCount = default)
    {
        Value = value;
        MaxValue = maxValue;
        Source = source;
        VoteCount = voteCount;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of <see cref="Rating"/>.
    /// </summary>
    /// <param name="value">The numeric value of the rating.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="source">The optional source of the rating.</param>
    /// <param name="voteCount">The optional number of votes or reviews.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing either a successfully created <see cref="Rating"/> or an error message.
    /// </returns>
    public static ErrorOr<Rating> Create(decimal value, decimal maxValue, Optional<RatingSource> source = default, Optional<int> voteCount = default)
    {
        if (maxValue < 0 || value < 0)
            return Errors.Errors.Metadata.RatingMustBePositive;
        if (value > maxValue)
            return Errors.Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue;
        return new Rating(value, maxValue, source, voteCount);
    }

    /// <summary>
    /// Returns the rating as a percentage.
    /// </summary>
    /// <returns>The rating value as a percentage of the maximum value.</returns>
    public decimal AsPercentage()
    {
        return (Value / MaxValue) * 100;
    }

    /// <inheritdoc/>
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
    #endregion
}