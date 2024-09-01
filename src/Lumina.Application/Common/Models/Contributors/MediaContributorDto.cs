namespace Lumina.Application.Common.Models.Contributors;

/// <summary>
/// Represents a media contributor.
/// </summary>
public record MediaContributorDto(
    string? Name,
    MediaContributorRoleDto? Role
);