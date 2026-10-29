#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Infrastructure.Authorization.Policies.LibraryOwnership;

/// <summary>
/// Fixture class for the <see cref="LibraryOwnershipPolicyContext"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryOwnershipPolicyContextFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LibraryOwnershipPolicyContext"/> instance with randomized test data.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose access is being evaluated.</param>
    /// <returns>The created <see cref="LibraryOwnershipPolicyContext"/> instance.</returns>
    public LibraryOwnershipPolicyContext Create(Guid? libraryId = null)
    {
        return new LibraryOwnershipPolicyContext(libraryId ?? _faker.Random.Guid());
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryOwnershipPolicyContext"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryOwnershipPolicyContext"/> instances.</returns>
    public List<LibraryOwnershipPolicyContext> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
