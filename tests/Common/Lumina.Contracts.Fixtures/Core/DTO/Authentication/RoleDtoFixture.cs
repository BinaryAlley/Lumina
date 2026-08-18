#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Authentication;

/// <summary>
/// Fixture class for the <see cref="RoleDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RoleDto"/>.
    /// </summary>
    /// <param name="id">Optional. The unique identifier of the role.</param>
    /// <param name="roleName">Optional. The name of the role.</param>
    /// <returns>The created <see cref="RoleDto"/>.</returns>
    public RoleDto Create(
        Guid? id = null,
        string? roleName = null)
    {
        return new RoleDto(
            id ?? Guid.NewGuid(),
            roleName ?? _faker.Lorem.Word());
    }

    /// <summary>
    /// Creates a list of <see cref="RoleDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RoleDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
