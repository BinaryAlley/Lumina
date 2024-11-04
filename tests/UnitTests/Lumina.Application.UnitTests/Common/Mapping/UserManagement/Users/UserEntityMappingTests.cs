#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Contracts.Responses.UsersManagement.Users;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserEntityMappingTests
{
    [Fact]
    public void ToResponse_WhenMappingValidUserEntity_ShouldMapCorrectly()
    {
        // Arrange
        UserEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Password = "hashedpassword",
            Libraries = [],
            Created = DateTime.UtcNow,
            Updated = null
        };

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Username.Should().Be(entity.Username);
        result.Created.Should().Be(entity.Created);
        result.Updated.Should().Be(entity.Updated);
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("test.user")]
    [InlineData("john_doe")]
    [InlineData("jane.doe@example.com")]
    public void ToResponse_WhenMappingDifferentUsernames_ShouldMapCorrectly(string username)
    {
        // Arrange
        UserEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Username = username,
            Password = "hashedpassword",
            Libraries = [],
            Created = DateTime.UtcNow,
            Updated = null
        };

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
    }

    [Fact]
    public void ToResponse_WhenMappingWithUpdatedDateTime_ShouldMapCorrectly()
    {
        // Arrange
        DateTime updated = DateTime.UtcNow.AddDays(-1);
        UserEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Password = "hashedpassword",
            Libraries = [],
            Created = DateTime.UtcNow,
            Updated = updated
        };

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Updated.Should().Be(updated);
    }

    [Fact]
    public void ToResponse_WhenMappingWithLibraries_ShouldNotAffectMapping()
    {
        // Arrange
        UserEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Password = "hashedpassword",
            Libraries =
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
            ],
            Created = DateTime.UtcNow,
            Updated = null
        };

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Username.Should().Be(entity.Username);
        result.Created.Should().Be(entity.Created);
        result.Updated.Should().Be(entity.Updated);
    }
}
