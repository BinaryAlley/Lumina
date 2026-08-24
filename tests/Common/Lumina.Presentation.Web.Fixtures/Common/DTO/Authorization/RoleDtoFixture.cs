#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;

/// <summary>
/// Fixture class for generating <see cref="RoleDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="RoleDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional identifier of the role.</param>
    /// <param name="roleName">Optional name of the role.</param>
    /// <returns>A configured <see cref="RoleDto"/> instance.</returns>
    public RoleDto Create(Guid? id = null, string? roleName = null)
    {
        return new RoleDto(
            Id: id ?? Guid.NewGuid(),
            RoleName: roleName ?? _faker.Commerce.Department()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="RoleDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RoleDto"/> instances.</returns>
    public List<RoleDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
