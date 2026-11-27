#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Authorization;

/// <summary>
/// Fixture class for the <see cref="RoleResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RoleResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the role.</param>
    /// <param name="roleName">Optional. The name of the role.</param>
    /// <returns>The created <see cref="RoleResponse"/>.</returns>
    public RoleResponse Create(
        Guid? id = null, 
        string? roleName = null)
    {
        return new RoleResponse(id ?? Guid.NewGuid(), roleName ?? _faker.Commerce.Department());
    }

    /// <summary>
    /// Creates a list of <see cref="RoleResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RoleResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
