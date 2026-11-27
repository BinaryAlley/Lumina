#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="MediaContributorReferenceDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorReferenceDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaContributorReferenceDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="contributorId">Optional. The unique identifier of the media contributor.</param>
    /// <param name="role">Optional. The role the contributor played.</param>
    /// <param name="includeRole">Whether the role should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MediaContributorReferenceDto"/> instance.</returns>
    public MediaContributorReferenceDto Create(
        Guid? contributorId = null,
        MediaContributorRole? role = null,
        bool includeRole = true)
    {
        return new MediaContributorReferenceDto
        {
            ContributorId = contributorId ?? _faker.Random.Guid(),
            Role = includeRole ? (role ?? _faker.PickRandom<MediaContributorRole>()) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MediaContributorReferenceDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorReferenceDto"/> instances.</returns>
    public List<MediaContributorReferenceDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
