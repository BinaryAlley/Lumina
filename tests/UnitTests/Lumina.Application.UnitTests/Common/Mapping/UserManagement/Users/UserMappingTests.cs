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
            VerificationToken = "verificationtoken",
            VerificationTokenCreated = DateTime.UtcNow,
            Libraries = [],
            Created = DateTime.UtcNow,
            Updated = null
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
        result.VerificationToken.Should().Be(repositoryEntity.VerificationToken);
        result.VerificationTokenCreated.Should().Be(repositoryEntity.VerificationTokenCreated);
        result.Libraries.Should().BeSameAs(repositoryEntity.Libraries);
        result.Created.Should().Be(repositoryEntity.Created);
        result.Updated.Should().Be(repositoryEntity.Updated);
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
            Created = DateTime.UtcNow,
            Updated = null
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
                Created = DateTime.UtcNow,
                Updated = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Library 2",
                LibraryType = LibraryType.Movie,
                ContentLocations = [],
                Created = DateTime.UtcNow,
                Updated = null
            }
        ];

        UserEntity repositoryEntity = new()
        {
            Id = Guid.NewGuid(),
            Username = "oldusername",
            Password = "hashedpassword",
            Libraries = libraries,
            Created = DateTime.UtcNow,
            Updated = null
        };

        // Act
        UserEntity result = domainEntity.ToRepositoryEntity(repositoryEntity);

        // Assert
        result.Should().NotBeNull();
        result.Libraries.Should().BeSameAs(libraries);
        result.Libraries.Should().HaveCount(2);
    }
}
