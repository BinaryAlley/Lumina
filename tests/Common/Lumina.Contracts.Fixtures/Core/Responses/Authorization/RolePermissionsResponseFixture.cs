#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Fixtures.Core.DTO.Authentication;
using Lumina.Contracts.Responses.Authorization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Authorization;

/// <summary>
/// Fixture class for the <see cref="RolePermissionsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionsResponseFixture
{
    private readonly RoleDtoFixture _roleDtoFixture = new();
    private readonly PermissionDtoFixture _permissionDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="RolePermissionsResponse"/>.
    /// </summary>
    /// <param name="role">Optional. The authorization role.</param>
    /// <param name="permissions">Optional. The permissions of the authorization role.</param>
    /// <returns>The created <see cref="RolePermissionsResponse"/>.</returns>
    public RolePermissionsResponse Create(
        RoleDto? role = null,
        IEnumerable<PermissionDto>? permissions = null)
    {
        return new RolePermissionsResponse(
            role ?? _roleDtoFixture.Create(),
            (permissions ?? _permissionDtoFixture.CreateMany()).ToArray());
    }

    /// <summary>
    /// Creates a list of <see cref="RolePermissionsResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RolePermissionsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
