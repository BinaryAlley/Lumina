#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaContributorRole"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorRoleFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorRole"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the role. If not provided, a random name is generated.</param>
    /// <param name="category">Optional. The category of the role. If not provided, a random category is generated.</param>
    /// <returns>The created <see cref="MediaContributorRole"/>.</returns>
    public MediaContributorRole Create(string? name = null, MediaContributorRoleCategory? category = null)
    {
        return MediaContributorRole.Create(
            name ?? _faker.Lorem.Word(),
            category ?? _faker.PickRandom<MediaContributorRoleCategory>()).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MediaContributorRole"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorRole"/> instances.</returns>
    public List<MediaContributorRole> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
