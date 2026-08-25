#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;

/// <summary>
/// Fixture class for generating <see cref="PermissionDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PermissionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional unique identifier of the permission.</param>
    /// <param name="permission">Optional name of the permission.</param>
    /// <returns>A configured <see cref="PermissionDto"/> instance.</returns>
    public PermissionDto Create(
        Guid? id = null,
        AuthorizationPermission? permission = null)
    {
        return new PermissionDto(
            Id: id ?? Guid.NewGuid(),
            PermissionName: permission ?? _faker.Random.Enum<AuthorizationPermission>()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PermissionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PermissionDto"/> instances.</returns>
    public List<PermissionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
