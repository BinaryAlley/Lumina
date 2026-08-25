#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;

/// <summary>
/// Fixture class for the <see cref="UserEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserEntity"/>.
    /// </summary>
    /// <param name="libraryCount">Number of libraries to generate. Default is 0.</param>
    /// <param name="username">Optional username to pin, or <see langword="null"/> to generate a random one.</param>
    /// <param name="password">Optional password to pin, or <see langword="null"/> to generate a random one.</param>
    /// <param name="id">Optional Id to pin, or <see langword="null"/> to generate a random one.</param>
    /// <param name="userRole">Optional user role association to pin when including it.</param>
    /// <param name="userPermissions">Optional user permission associations to pin when including them.</param>
    /// <param name="includeUserRole">Whether the user should include a user role association or not.</param>
    /// <param name="includeUserPermissions">Whether the user should include user permission associations or not.</param>
    /// <param name="libraries">Optional collection of libraries to pin for the user, or <see langword="null"/> to generate them based on <paramref name="libraryCount"/>.</param>
    /// <returns>The created user entity.</returns>
    public UserEntity Create(
        int libraryCount = 0,
        string? username = null,
        string? password = null,
        Guid? id = null,
        UserRoleEntity? userRole = null,
        IEnumerable<UserPermissionEntity>? userPermissions = null,
        bool includeUserRole = false,
        bool includeUserPermissions = false,
        ICollection<LibraryEntity>? libraries = null)
    {
        Guid userId = id ?? Guid.NewGuid();
        ICollection<LibraryEntity> resolvedLibraries = libraries ?? (libraryCount > 0
            ? CreateLibraries(libraryCount, userId)
            : []);

        return new Faker<UserEntity>()
            .CustomInstantiator(f => new UserEntity
            {
                Id = userId,
                Username = default!,
                Password = default!,
                CreatedOnUtc = default,
                TotpSecret = default,
                Libraries = resolvedLibraries,
                UserPermissions = includeUserPermissions ? (userPermissions ?? []).ToList() : [],
                UserRole = includeUserRole ? userRole : null,
                CreatedBy = userId
            })
            .RuleFor(x => x.Username, f => username ?? f.Internet.UserName())
            .RuleFor(x => x.Password, f => password ?? Uri.EscapeDataString(f.Internet.Password()))
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.TotpSecret, f => f.Random.Bool() ? f.Random.String2(32) : null)
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UserEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <param name="libraryCount">Number of libraries to generate for each user.</param>
    /// <returns>List of configured <see cref="UserEntity"/> instances.</returns>
    public List<UserEntity> CreateMany(int count = 3, int libraryCount = 0)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create(libraryCount))];
    }

    /// <summary>
    /// Creates a valid collection of user libraries.
    /// </summary>
    /// <param name="count">The number of user libraries to create.</param>
    /// <param name="userId">The unique identifier of the user for whom to create the libraries.</param>
    /// <returns>A collection of <see cref="LibraryEntity"/>.</returns>
    private static List<LibraryEntity> CreateLibraries(int count, Guid userId)
    {
        return new Faker<LibraryEntity>()
            .CustomInstantiator(f => new LibraryEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = f.Commerce.ProductName(),
                LibraryType = f.PickRandom<LibraryType>(),
                ContentLocations = [],
                CreatedOnUtc = f.Date.Past(),
                CreatedBy = userId,
                UpdatedOnUtc = null,
                UpdatedBy = null
            })
            .Generate(count);
    }
}
