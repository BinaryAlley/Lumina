#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Authentication;

/// <summary>
/// Fixture class for the <see cref="PermissionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PermissionDto"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the permission.</param>
    /// <param name="permissionName">Optional. The name of the permission.</param>
    /// <returns>The created <see cref="PermissionDto"/>.</returns>
    public PermissionDto Create(
        Guid? id = null,
        AuthorizationPermission? permissionName = null)
    {
        return new PermissionDto(
            id ?? Guid.NewGuid(),
            permissionName ?? _faker.PickRandom<AuthorizationPermission>()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="PermissionDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PermissionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
