#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="MediaContributorNameDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorNameDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaContributorNameDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="displayName">Optional. The name by which the contributor is popularly known.</param>
    /// <param name="legalName">Optional. The legal name of the contributor.</param>
    /// <returns>A configured <see cref="MediaContributorNameDto"/> instance.</returns>
    public MediaContributorNameDto Create(
        string? displayName = null,
        string? legalName = null)
    {
        return new MediaContributorNameDto
        {
            DisplayName = displayName ?? _faker.Name.FullName(),
            LegalName = legalName ?? _faker.Name.FullName()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MediaContributorNameDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorNameDto"/> instances.</returns>
    public List<MediaContributorNameDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
