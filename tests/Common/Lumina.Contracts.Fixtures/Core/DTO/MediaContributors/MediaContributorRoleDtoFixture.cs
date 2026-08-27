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
/// Fixture class for the <see cref="MediaContributorRoleDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorRoleDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorRoleDto"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the role assigned to the media contributor.</param>
    /// <param name="category">Optional. The category of the role.</param>
    /// <returns>The created <see cref="MediaContributorRoleDto"/>.</returns>
    public MediaContributorRoleDto Create(
        string? name = null,
        MediaContributorRoleCategory? category = null)
    {
        return new MediaContributorRoleDto(
            name ?? _faker.Commerce.Department(),
            category ?? _faker.PickRandom<MediaContributorRoleCategory>());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaContributorRoleDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaContributorRoleDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
