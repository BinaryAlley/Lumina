namespace Lumina.Application.Common.Models.Common;

/// <summary>
/// Represents a request to get release information.
/// </summary>
public record ReleaseInfoDto(
    DateOnly? OriginalReleaseDate,
    int? OriginalReleaseYear,
    DateOnly? ReReleaseDate,
    int? ReReleaseYear,
    string? ReleaseCountry,
    string? ReleaseVersion
);