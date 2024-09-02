namespace Lumina.Application.Common.Models.Contributors;

/// <summary>
/// Represents a media contributor name.
/// </summary>
public record MediaContributorNameDto(
    string? DisplayName,
    string? LegalName
);