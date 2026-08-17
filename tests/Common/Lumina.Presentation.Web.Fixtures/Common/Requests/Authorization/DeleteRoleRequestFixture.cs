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
/// Fixture class for generating <see cref="DeleteRoleRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="DeleteRoleRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="roleId">Optional identifier of the role.</param>
    /// <returns>A configured <see cref="DeleteRoleRequest"/> instance.</returns>
    public DeleteRoleRequest Create(Guid? roleId = null)
    {
        return new DeleteRoleRequest(
            RoleId: roleId ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeleteRoleRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteRoleRequest"/> instances.</returns>
    public List<DeleteRoleRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
