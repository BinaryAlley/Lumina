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
/// Fixture class for the <see cref="UpdateRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdateRoleRequest"/>.
    /// </summary>
    /// <param name="roleId">Optional. The Id of the role to update.</param>
    /// <param name="roleName">Optional. The name of the role.</param>
    /// <param name="permissions">Optional. The Ids of the permissions of the role.</param>
    /// <returns>The created <see cref="UpdateRoleRequest"/>.</returns>
    public UpdateRoleRequest Create(
        Guid? roleId = null,
        string? roleName = null,
        List<Guid>? permissions = null)
    {
        return new UpdateRoleRequest(
            roleId ?? Guid.NewGuid(),
            roleName ?? _faker.Random.Word(),
            permissions ?? [Guid.NewGuid(), Guid.NewGuid()]
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateRoleRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateRoleRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
