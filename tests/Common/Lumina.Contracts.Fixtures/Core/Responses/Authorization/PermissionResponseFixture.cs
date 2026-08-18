#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Authorization;

/// <summary>
/// Fixture class for the <see cref="PermissionResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PermissionResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the permission.</param>
    /// <param name="permissionName">Optional. The name of the permission.</param>
    /// <returns>The created <see cref="PermissionResponse"/>.</returns>
    public PermissionResponse Create(
        Guid? id = null,
        AuthorizationPermission? permissionName = null)
    {
        return new PermissionResponse(
            id ?? Guid.NewGuid(),
            permissionName ?? _faker.PickRandom<AuthorizationPermission>()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="PermissionResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PermissionResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
