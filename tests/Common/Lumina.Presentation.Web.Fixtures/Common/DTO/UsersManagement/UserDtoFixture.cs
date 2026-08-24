#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;

/// <summary>
/// Fixture class for generating <see cref="UserDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UserDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional identifier of the user.</param>
    /// <param name="username">Optional username of the user.</param>
    /// <returns>A configured <see cref="UserDto"/> instance.</returns>
    public UserDto Create(
        Guid? id = null, 
        string? username = null)
    {
        return new UserDto(
            Id: id ?? Guid.NewGuid(),
            Username: username ?? _faker.Internet.UserName()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="UserDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserDto"/> instances.</returns>
    public List<UserDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
