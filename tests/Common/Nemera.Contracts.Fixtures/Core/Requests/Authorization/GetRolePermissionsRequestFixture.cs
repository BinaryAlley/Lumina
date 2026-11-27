#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authorization;

/// <summary>
/// Fixture class for the <see cref="GetRolePermissionsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetRolePermissionsRequest"/>.
    /// </summary>
    /// <param name="roleId">The Id of the role whose permissions are retrieved.</param>
    /// <returns>The created <see cref="GetRolePermissionsRequest"/>.</returns>
    public GetRolePermissionsRequest Create(
        Guid? roleId = null)
    {
        return new GetRolePermissionsRequest(roleId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetRolePermissionsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetRolePermissionsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
