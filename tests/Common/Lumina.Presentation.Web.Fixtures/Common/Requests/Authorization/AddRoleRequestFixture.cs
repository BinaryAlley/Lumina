#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Authorization;

/// <summary>
/// Fixture class for generating <see cref="AddRoleRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="AddRoleRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="roleName">Optional name of the role.</param>
    /// <param name="permissions">Optional collection of permission identifiers.</param>
    /// <returns>A configured <see cref="AddRoleRequest"/> instance.</returns>
    public AddRoleRequest Create(string? roleName = null, List<Guid>? permissions = null)
    {
        Faker faker = new();
        return new AddRoleRequest(
            RoleName: roleName ?? faker.Commerce.Department(),
            Permissions: permissions ?? [Guid.NewGuid(), Guid.NewGuid()]
        );
    }

    /// <summary>
    /// Creates multiple <see cref="AddRoleRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddRoleRequest"/> instances.</returns>
    public List<AddRoleRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
