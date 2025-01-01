#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;

/// <summary>
/// Fixture class for the <see cref="UserEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserEntityFixture
{
    /// <summary>
    /// Creates a random valid user entity.
    /// </summary>
    /// <param name="libraryCount">Number of libraries to generate. Default is 0.</param>
    /// <returns>The created user entity.</returns>
    public static UserEntity CreateUserEntity(int libraryCount = 0)
    {
        Guid userId = Guid.NewGuid();
        List<LibraryEntity> libraries = [.. new Faker<LibraryEntity>()
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
            .Generate(libraryCount)];

        return new Faker<UserEntity>()
            .CustomInstantiator(f => new UserEntity
            {
                Id = userId,
                Username = default!,
                Password = default!,
                CreatedOnUtc = default,
                TotpSecret = default,
                Libraries = libraries,
                UserPermissions = [],
                UserRole = null,
                CreatedBy = userId
            })
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Password, f => Uri.EscapeDataString(f.Internet.Password()))
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.TotpSecret, f => f.Random.Bool() ? f.Random.String2(32) : null)
            .Generate();
    }

    /// <summary>
    /// Creates a list of user entities.
    /// </summary>
    /// <param name="count">The number of entities to create.</param>
    /// <returns>The created list.</returns>
    public static List<UserEntity> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateUserEntity())
            .ToList();
    }
}
