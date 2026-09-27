#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.Enums.MediaContributors;
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
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaContributorDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="displayName">Optional. The name by which the contributor is popularly known.</param>
    /// <param name="legalName">Optional. The legal name of the contributor.</param>
    /// <param name="role">Optional. The canonical role of the contributor.</param>
    /// <returns>A configured <see cref="MediaContributorDto"/> instance.</returns>
    public MediaContributorDto Create(
        string? displayName = null,
        string? legalName = null,
        MediaContributorRole? role = null)
    {
        return new MediaContributorDto
        {
            Name = new MediaContributorNameDto
            {
                DisplayName = displayName ?? _faker.Name.FullName(),
                LegalName = legalName ?? _faker.Name.FullName()
            },
            Role = role ?? MediaContributorRole.Author
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
