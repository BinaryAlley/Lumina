#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authorization;

/// <summary>
/// Fixture class for the <see cref="AddRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AddRoleRequest"/>.
    /// </summary>
    /// <param name="roleName">Optional. The name of the role.</param>
    /// <param name="permissions">Optional. The Ids of the permissions granted to the role.</param>
    /// <returns>The created <see cref="AddRoleRequest"/>.</returns>
    public AddRoleRequest Create(
        string? roleName = null, 
        List<Guid>? permissions = null)
    {
        return new AddRoleRequest(roleName ?? _faker.Commerce.Department(), permissions ?? []);
    }

    /// <summary>
    /// Creates a list of <see cref="AddRoleRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddRoleRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
