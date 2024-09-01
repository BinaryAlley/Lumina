namespace Lumina.Application.Common.Models.Common;

/// <summary>
/// Represents a rating.
/// </summary>
public abstract record RatingDto(
    decimal? Value,
    decimal? MaxValue,
    int? VoteCount
);