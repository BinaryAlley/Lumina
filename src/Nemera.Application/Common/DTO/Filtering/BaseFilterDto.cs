namespace Lumina.Application.Common.DTO.Filtering;

/// <summary>
/// Data transfer object that defines common filter properties used when querying paginated data. Implementations may add additional properties specific to their module or domain.
/// </summary>
public record BaseFilterDto
{
    /// <summary>
    /// The search term used to filter results.
    /// </summary>
    public string? SearchTerm { get; init; }
}
