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
    /// <returns>The created <see cref="MediaContributorDto"/>.</returns>
    public MediaContributorDto Create(
        string? displayName = null,
        string? roleName = null,
        MediaContributorRoleCategory? roleCategory = null)
    {
        return new MediaContributorDto(
            Name: new MediaContributorNameDto(DisplayName: displayName ?? _faker.Name.FullName(), LegalName: null),
            Role: new MediaContributorRoleDto(Name: roleName ?? _faker.Commerce.Department(), Category: roleCategory ?? _faker.PickRandom<MediaContributorRoleCategory>()));
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
