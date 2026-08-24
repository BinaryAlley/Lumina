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
/// Fixture class for generating <see cref="UpdateRoleRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UpdateRoleRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="roleId">Optional identifier of the role.</param>
    /// <param name="roleName">Optional name of the role.</param>
    /// <param name="permissions">Optional collection of permission identifiers.</param>
    /// <returns>A configured <see cref="UpdateRoleRequest"/> instance.</returns>
    public UpdateRoleRequest Create(
        Guid? roleId = null, 
        string? roleName = null, 
        List<Guid>? permissions = null)
    {
        return new UpdateRoleRequest(
            RoleId: roleId ?? Guid.NewGuid(),
            RoleName: roleName ?? _faker.Commerce.Department(),
            Permissions: permissions ?? [Guid.NewGuid(), Guid.NewGuid()]
        );
    }

    /// <summary>
    /// Creates multiple <see cref="UpdateRoleRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateRoleRequest"/> instances.</returns>
    public List<UpdateRoleRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
