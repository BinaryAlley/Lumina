#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.UsersManagement.Users;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.UsersManagement.Users;

/// <summary>
/// Fixture class for the <see cref="UserResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UserResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the user.</param>
    /// <param name="username">Optional. The username of the user.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the user was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the user was updated.</param>
    /// <returns>The created <see cref="UserResponse"/>.</returns>
    public UserResponse Create(
        Guid? id = null,
        string? username = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null)
    {
        return new UserResponse(
            id ?? Guid.NewGuid(),
            username ?? _faker.Internet.UserName(),
            createdOnUtc ?? _faker.Date.Past().ToUniversalTime(),
            updatedOnUtc
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UserResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UserResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
