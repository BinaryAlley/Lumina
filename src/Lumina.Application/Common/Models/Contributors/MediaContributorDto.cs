namespace Lumina.Application.Common.Models.Contributors;

/// <summary>
/// Represents a media contributor.
/// </summary>
public record MediaContributorDto(
    MediaContributorNameDto? Name,
    MediaContributorRoleDto? Role
);