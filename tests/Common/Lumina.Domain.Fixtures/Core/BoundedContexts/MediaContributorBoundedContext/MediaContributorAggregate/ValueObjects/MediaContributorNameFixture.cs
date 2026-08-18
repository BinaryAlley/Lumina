#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaContributorName"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorNameFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorName"/>.
    /// </summary>
    /// <param name="displayName">Optional. The name by which the contributor is popularly known. If not provided, a random name is generated.</param>
    /// <param name="legalName">Optional. The legal name of the contributor. If not provided, a random legal name is generated.</param>
    /// <returns>The created <see cref="MediaContributorName"/>.</returns>
    public MediaContributorName Create(string? displayName = null, Optional<string>? legalName = null)
    {
        Result<MediaContributorName> nameResult = MediaContributorName.Create(
            displayName ?? _faker.Person.FullName,
            legalName ?? Optional<string>.Some(_faker.Person.FullName));

        if (nameResult.IsFailure)
            throw new InvalidOperationException("Failed to create MediaContributorName: " + string.Join(", ", nameResult.Errors));
        return nameResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MediaContributorName"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorName"/> instances.</returns>
    public List<MediaContributorName> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
