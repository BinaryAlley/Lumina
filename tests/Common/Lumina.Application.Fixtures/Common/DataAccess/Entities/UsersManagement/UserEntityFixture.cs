#region ========================================================================= USING =====================================================================================
using Bogus;
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
    /// <returns>The created user entity.</returns>
    public UserEntity Create(int libraryCount = 0)
    {
        Guid userId = Guid.NewGuid();
        List<LibraryEntity> libraries = libraryCount > 0
            ? CreateLibraries(libraryCount, userId)
            : [];

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
