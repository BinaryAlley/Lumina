#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Authorization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;

/// <summary>
/// Fixture class for the <see cref="RolePermissionsDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionsDtoFixture
{
    private readonly RoleDtoFixture _roleDtoFixture = new();
    private readonly PermissionDtoFixture _permissionDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="RolePermissionsDto"/>.
    /// </summary>
    /// <param name="role">Optional. The authorization role.</param>
    /// <param name="permissions">Optional. The permissions of the authorization role.</param>
    /// <returns>The created <see cref="RolePermissionsDto"/>.</returns>
    public RolePermissionsDto Create(
        RoleDto? role = null, 
        PermissionDto[]? permissions = null)
    {
        return new RolePermissionsDto(
            role ?? _roleDtoFixture.Create(),
            permissions ?? [.. _permissionDtoFixture.CreateMany()]);
    }

    /// <summary>
    /// Creates a list of <see cref="RolePermissionsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RolePermissionsDto"/> instances.</returns>
    public List<RolePermissionsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
