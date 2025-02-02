#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
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
    [Fact]
    public void ToRepositoryEntity_WhenMappingValidUser_ShouldOnlyUpdateUsername()
    {
        // Arrange
        User domainEntity = User.Create(
            UserId.Create(Guid.NewGuid()),
            "newusername"
        ).Value;

        UserEntity repositoryEntity = new()
        {
            Id = Guid.NewGuid(),
            Username = "oldusername",
            Password = "hashedpassword",
            TotpSecret = "totpsecret",
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = null
        };

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
        User domainEntity = User.Create(
            UserId.Create(Guid.NewGuid()),
            newUsername
        ).Value;

        UserEntity repositoryEntity = new()
        {
            Id = Guid.NewGuid(),
            Username = "oldusername",
            Password = "hashedpassword",
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = null
        };

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
        User domainEntity = User.Create(
            UserId.Create(Guid.NewGuid()),
            "newusername"
        ).Value;

        List<LibraryEntity> libraries =
        [
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Library 1",
                LibraryType = LibraryType.Book,
                ContentLocations = [],
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                UpdatedOnUtc = null,
                UpdatedBy = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Library 2",
                LibraryType = LibraryType.Movie,
                ContentLocations = [],
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                UpdatedOnUtc = null,
                UpdatedBy = null
            }
        ];

        UserEntity repositoryEntity = new()
        {
            Id = Guid.NewGuid(),
            Username = "oldusername",
            Password = "hashedpassword",
            Libraries = libraries,
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = null
        };

        // Act
        UserEntity result = domainEntity.ToRepositoryEntity(repositoryEntity);

        // Assert
        Assert.NotNull(result);
        Assert.Same(libraries, result.Libraries);
        Assert.Equal(2, result.Libraries.Count);
    }
}
