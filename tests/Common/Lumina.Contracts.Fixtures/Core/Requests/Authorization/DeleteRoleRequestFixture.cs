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
/// Fixture class for the <see cref="DeleteRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeleteRoleRequest"/>.
    /// </summary>
    /// <param name="roleId">Optional. The Id of the role to delete.</param>
    /// <returns>The created <see cref="DeleteRoleRequest"/>.</returns>
    public DeleteRoleRequest Create(
        Guid? roleId = null)
    {
        return new DeleteRoleRequest(roleId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteRoleRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<DeleteRoleRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
