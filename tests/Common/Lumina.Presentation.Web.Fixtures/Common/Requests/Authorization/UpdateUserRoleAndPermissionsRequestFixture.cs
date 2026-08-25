#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Authorization;

/// <summary>
/// Fixture class for generating <see cref="UpdateUserRoleAndPermissionsRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="UpdateUserRoleAndPermissionsRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional identifier of the user.</param>
    /// <param name="roleId">Optional identifier of the role assigned to the user.</param>
    /// <param name="permissions">Optional collection of permission identifiers assigned to the user.</param>
    /// <returns>A configured <see cref="UpdateUserRoleAndPermissionsRequest"/> instance.</returns>
    public UpdateUserRoleAndPermissionsRequest Create(
        Guid? userId = null, 
        Guid? roleId = null, 
        List<Guid>? permissions = null)
    {
        return new UpdateUserRoleAndPermissionsRequest(
            UserId: userId ?? Guid.NewGuid(),
            RoleId: roleId ?? Guid.NewGuid(),
            Permissions: permissions ?? [Guid.NewGuid(), Guid.NewGuid()]
        );
    }

    /// <summary>
    /// Creates multiple <see cref="UpdateUserRoleAndPermissionsRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateUserRoleAndPermissionsRequest"/> instances.</returns>
    public List<UpdateUserRoleAndPermissionsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
