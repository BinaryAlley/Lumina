#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Should().BeSameAs(repositoryEntity); // Should modify the same instance
        result.Username.Should().Be(domainEntity.Username); // Only username should be updated

        // Verify other properties remain unchanged
        result.Id.Should().Be(repositoryEntity.Id);
        result.Password.Should().Be(repositoryEntity.Password);
        result.TotpSecret.Should().Be(repositoryEntity.TotpSecret);
        result.Libraries.Should().BeSameAs(repositoryEntity.Libraries);
        result.UserPermissions.Should().BeSameAs(repositoryEntity.UserPermissions);
        result.UserRole.Should().BeSameAs(repositoryEntity.UserRole);
        result.CreatedOnUtc.Should().Be(repositoryEntity.CreatedOnUtc);
        result.CreatedBy.Should().Be(repositoryEntity.CreatedBy);
        result.UpdatedOnUtc.Should().Be(repositoryEntity.UpdatedOnUtc);
        result.UpdatedBy.Should().Be(repositoryEntity.UpdatedBy);
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
        result.Should().NotBeNull();
        result.Username.Should().Be(newUsername);
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
        result.Should().NotBeNull();
        result.Libraries.Should().BeSameAs(libraries);
        result.Libraries.Should().HaveCount(2);
    }
}
