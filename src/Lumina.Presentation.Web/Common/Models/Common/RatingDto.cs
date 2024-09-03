namespace Lumina.Presentation.Web.Common.Models.Common;

/// <summary>
/// Represents a rating.
/// </summary>
public abstract class RatingDto
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the numeric value of the rating.
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// Gets the maximum possible rating value.
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Gets the optional number of votes or reviews this rating is based on.
    /// </summary>
    public int? VoteCount { get; set; }
    #endregion
}