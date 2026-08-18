#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;

/// <summary>
/// Fixture class for the <see cref="User"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="User"/> domain aggregate.
    /// </summary>
    /// <param name="id">Optional. The user Id.</param>
    /// <param name="username">Optional. The username of the user.</param>
    /// <returns>The created <see cref="User"/>.</returns>
    public User Create(Guid? id = null, string? username = null)
    {
        UserId userId = id is null ? UserId.CreateUnique() : UserId.Create(id.Value);
        Result<User> user = User.Create(userId, username ?? _faker.Internet.UserName());
        return user.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="User"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="User"/> instances.</returns>
    public List<User> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
