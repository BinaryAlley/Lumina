#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="MediaContributorDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorDto"/>.
    /// </summary>
    /// <param name="displayName">Optional. The display name of the media contributor.</param>
    /// <param name="roleName">Optional. The name of the role assigned to the media contributor.</param>
    /// <param name="roleCategory">Optional. The category of the role.</param>
    /// <param name="legalName">Optional. The legal name of the media contributor.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDisplayName">Whether the display name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLegalName">Whether the legal name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRole">Whether the role should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRoleName">Whether the role name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRoleCategory">Whether the role category should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MediaContributorDto"/>.</returns>
    public MediaContributorDto Create(
        string? displayName = null,
        string? roleName = null,
        MediaContributorRoleCategory? roleCategory = null,
        string? legalName = null,
        bool includeName = true,
        bool includeDisplayName = true,
        bool includeLegalName = true,
        bool includeRole = true,
        bool includeRoleName = true,
        bool includeRoleCategory = true)
    {
        return new MediaContributorDto(
            Name: includeName ? new MediaContributorNameDto(
                DisplayName: includeDisplayName ? displayName ?? _faker.Name.FullName() : null,
                LegalName: includeLegalName ? legalName : null) : null,
            Role: includeRole ? new MediaContributorRoleDto(
                Name: includeRoleName ? roleName ?? _faker.Commerce.Department() : null,
                Category: includeRoleCategory ? roleCategory ?? _faker.PickRandom<MediaContributorRoleCategory>() : null) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="MediaContributorDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaContributorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
