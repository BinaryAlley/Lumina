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
/// Fixture class for the <see cref="UpdateUserRoleAndPermissionsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdateUserRoleAndPermissionsRequest"/>.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user whose role and permissions are updated.</param>
    /// <param name="roleId">Optional. The Id of the role assigned to the user.</param>
    /// <param name="permissions">Optional. The Ids of the permissions assigned to the user.</param>
    /// <returns>The created <see cref="UpdateUserRoleAndPermissionsRequest"/>.</returns>
    public UpdateUserRoleAndPermissionsRequest Create(
        Guid? userId = null,
        Guid? roleId = null,
        List<Guid>? permissions = null)
    {
        return new UpdateUserRoleAndPermissionsRequest(
            userId ?? Guid.NewGuid(),
            roleId ?? Guid.NewGuid(),
            permissions ?? [Guid.NewGuid(), Guid.NewGuid()]
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateUserRoleAndPermissionsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateUserRoleAndPermissionsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
