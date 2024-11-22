#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Users.Fixtures;

/// <summary>
/// Fixture class for the <see cref="UserEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserEntityFixture
{
    private readonly Fixture _fixture;
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserEntityFixture"/> class.
    /// </summary>
    public UserEntityFixture()
    {
        _fixture = new Fixture();
        _faker = new Faker();
        ConfigureCustomTypes();
    }

    /// <summary>
    /// Creates a random valid <see cref="UserEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="UserEntity"/>.</returns>
    public UserEntity CreateUserModel()
    {
        Guid userId = _faker.Random.Guid();

        return new Faker<UserEntity>()
            .RuleFor(x => x.Id, _ => userId)
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Password, f => f.Internet.Password(12))
            .RuleFor(x => x.TempPassword, f => f.Random.Bool() ? f.Internet.Password(8) : null)
            .RuleFor(x => x.TotpSecret, f => f.Random.Bool() ? f.Random.String2(32, "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567") : null)
            .RuleFor(x => x.TempPasswordCreated, (f, u) => u.TempPassword != null ? f.Date.Past() : null)
            .RuleFor(x => x.Libraries, _ => CreateLibraries(_faker.Random.Number(1, 3), userId))
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.CreatedBy, _ => userId)
            .RuleFor(x => x.UpdatedOnUtc, f => f.Random.Bool() ? f.Date.Recent() : null)
            .RuleFor(x => x.UpdatedBy, _ => userId)
            .Generate();
    }

    /// <summary>
    /// Creates a valid collection of user libraries.
    /// </summary>
    /// <param name="count">The number of user libraries to create.</param>
    /// <param name="userId">The Id of the user for whom to create the libraries.</param>
    /// <returns>A collection of <see cref="LibraryEntity"/>.</returns>
    private static List<LibraryEntity> CreateLibraries(int count, Guid userId)
    {
        return new Faker<LibraryEntity>()
            .CustomInstantiator(f => new LibraryEntity
            {
                Id = f.Random.Guid(),
                UserId = userId,
                Title = f.Random.String2(f.Random.Number(1, 50)),
                LibraryType = f.PickRandom<LibraryType>(),
                ContentLocations = CreateContentLocations(f.Random.Number(1, 3)),
                Created = f.Date.Past(),
                Updated = f.Random.Bool() ? f.Date.Recent() : null
            })
            .Generate(count);
    }

    /// <summary>
    /// Creates a valid collection of media paths for a library.
    /// </summary>
    /// <param name="count">The number of media library contents to create.</param>
    /// <returns>A collection of <see cref="LibraryContentLocationEntity"/>.</returns>
    private static List<LibraryContentLocationEntity> CreateContentLocations(int count)
    {
        return new Faker<LibraryContentLocationEntity>()
            .CustomInstantiator(f => new LibraryContentLocationEntity
            {
                Path = f.System.FilePath()
            })
            .Generate(count);
    }

    /// <summary>
    /// Configures custom type generation rules for the AutoFixture instance.
    /// </summary>
    private void ConfigureCustomTypes()
    {
        _fixture.Register(() => new LibraryEntity
        {
            Id = _fixture.Create<Guid>(),
            UserId = _fixture.Create<Guid>(),
            Title = _fixture.Create<string>(),
            LibraryType = _fixture.Create<LibraryType>(),
            ContentLocations = new List<LibraryContentLocationEntity>(),
            Created = _fixture.Create<DateTime>(),
            Updated = _fixture.Create<DateTime>()
        });

        _fixture.Register(() => new LibraryContentLocationEntity
        {
            Path = _fixture.Create<string>()
        });
    }
}
