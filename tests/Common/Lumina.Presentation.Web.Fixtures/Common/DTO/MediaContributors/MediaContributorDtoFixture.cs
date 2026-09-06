#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;

/// <summary>
/// Fixture class for generating <see cref="MediaContributorDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorDtoFixture
{
    private static readonly string[] RoleCategories = ["Author", "Translator", "Illustrator", "Editor", "Narrator"];
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaContributorDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="displayName">Optional. The name by which the contributor is popularly known.</param>
    /// <param name="legalName">Optional. The legal name of the contributor.</param>
    /// <param name="roleName">Optional. The name of the role of the contributor.</param>
    /// <param name="roleCategory">Optional. The category of the role of the contributor.</param>
    /// <returns>A configured <see cref="MediaContributorDto"/> instance.</returns>
    public MediaContributorDto Create(
        string? displayName = null,
        string? legalName = null,
        string? roleName = null,
        string? roleCategory = null)
    {
        string resolvedRoleCategory = roleCategory ?? _faker.Random.ArrayElement(RoleCategories);
        return new MediaContributorDto
        {
            Name = new MediaContributorNameDto
            {
                DisplayName = displayName ?? _faker.Name.FullName(),
                LegalName = legalName ?? _faker.Name.FullName()
            },
            Role = new MediaContributorRoleDto
            {
                Name = roleName ?? resolvedRoleCategory.ToLowerInvariant(),
                Category = resolvedRoleCategory
            }
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MediaContributorDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorDto"/> instances.</returns>
    public List<MediaContributorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
