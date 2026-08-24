#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserMappingTests
{
    private readonly UserFixture _userFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingValidUser_ShouldOnlyUpdateUsername()
    {
        // Arrange
        User domainEntity = _userFixture.Create(username: "newusername");

        UserEntity repositoryEntity = _userEntityFixture.Create(username: "oldusername", password: "hashedpassword");

        // Act
        UserEntity result = domainEntity.ToRepositoryEntity(repositoryEntity);

        // Assert
        Assert.NotNull(result);
        Assert.Same(repositoryEntity, result); // should modify the same instance
        Assert.Equal(domainEntity.Username, result.Username); // only username should be updated

        // verify other properties remain unchanged
        Assert.Equal(repositoryEntity.Id, result.Id);
        Assert.Equal(repositoryEntity.Password, result.Password);
        Assert.Equal(repositoryEntity.TotpSecret, result.TotpSecret);
        Assert.Same(repositoryEntity.Libraries, result.Libraries);
        Assert.Same(repositoryEntity.UserPermissions, result.UserPermissions);
        Assert.Same(repositoryEntity.UserRole, result.UserRole);
        Assert.Equal(repositoryEntity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(repositoryEntity.CreatedBy, result.CreatedBy);
        Assert.Equal(repositoryEntity.UpdatedOnUtc, result.UpdatedOnUtc);
        Assert.Equal(repositoryEntity.UpdatedBy, result.UpdatedBy);
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("test.user")]
    [InlineData("john_doe")]
    [InlineData("jane.doe@example.com")]
    public void ToRepositoryEntity_WhenMappingDifferentUsernames_ShouldUpdateCorrectly(string newUsername)
    {
        // Arrange
        User domainEntity = _userFixture.Create(username: newUsername);

        UserEntity repositoryEntity = _userEntityFixture.Create(username: "oldusername", password: "hashedpassword");

        // Act
        UserEntity result = domainEntity.ToRepositoryEntity(repositoryEntity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newUsername, result.Username);
    }

    [Fact]
    public void ToRepositoryEntity_WhenRepositoryEntityHasLibraries_ShouldPreserveLibraries()
    {
        // Arrange
        User domainEntity = _userFixture.Create(username: "newusername");

        List<LibraryEntity> libraries =
        [
            _libraryEntityFixture.Create(title: "Library 1", libraryType: LibraryType.Book, contentLocations: []),
            _libraryEntityFixture.Create(title: "Library 2", libraryType: LibraryType.Movie, contentLocations: [])
        ];

        UserEntity repositoryEntity = _userEntityFixture.Create(
            username: "oldusername",
            password: "hashedpassword",
            libraries: libraries);

        // Act
        UserEntity result = domainEntity.ToRepositoryEntity(repositoryEntity);

        // Assert
        Assert.NotNull(result);
        Assert.Same(libraries, result.Libraries);
        Assert.Equal(2, result.Libraries.Count);
    }
}
