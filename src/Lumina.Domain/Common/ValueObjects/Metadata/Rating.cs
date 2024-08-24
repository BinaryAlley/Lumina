#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the rating of a media element.
/// </summary>
[DebuggerDisplay("{Value}/{MaxValue}")]
public abstract class Rating : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the numeric value of the rating.
    /// </summary>
    public decimal Value { get; private set; }

    /// <summary>
    /// Gets the maximum possible rating value.
    /// </summary>
    public decimal MaxValue { get; private set; }

    /// <summary>
    /// Gets the optional number of votes or reviews this rating is based on.
    /// </summary>
    public Optional<int> VoteCount { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="Rating"/> class.
    /// </summary>
    /// <param name="value">The numeric value of the rating.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="voteCount">The optional number of votes or reviews.</param>
    protected Rating(decimal value, decimal maxValue, Optional<int> voteCount)
    {
        Value = value;
        MaxValue = maxValue;
        VoteCount = voteCount;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Returns the rating as a percentage.
    /// </summary>
    /// <returns>The rating value as a percentage of the maximum value.</returns>
    public decimal AsPercentage()
    {
        return (Value / MaxValue) * 100;
    }
    #endregion
}