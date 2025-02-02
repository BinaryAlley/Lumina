#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Contracts.Responses.UsersManagement.Users;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        UserEntity entity = UserEntityFixture.CreateUserEntity();

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Username, result.Username);
        Assert.Equal(entity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, result.UpdatedOnUtc);
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("test.user")]
    [InlineData("john_doe")]
    [InlineData("jane.doe@example.com")]
    public void ToResponse_WhenMappingDifferentUsernames_ShouldMapCorrectly(string username)
    {
        // Arrange
        UserEntity entity = UserEntityFixture.CreateUserEntity();
        entity.Username = username;

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
    }

    [Fact]
    public void ToResponse_WhenMappingWithUpdatedDateTime_ShouldMapCorrectly()
    {
        // Arrange
        UserEntity entity = UserEntityFixture.CreateUserEntity();
        entity.UpdatedOnUtc = DateTime.UtcNow.AddDays(-1);

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.UpdatedOnUtc, result.UpdatedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenMappingWithLibraries_ShouldNotAffectMapping()
    {
        // Arrange
        UserEntity entity = UserEntityFixture.CreateUserEntity(2);

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Username, result.Username);
        Assert.Equal(entity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, result.UpdatedOnUtc);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleEntities_ShouldMapCorrectly()
    {
        // Arrange
        List<UserEntity> entities = UserEntityFixture.CreateMany(3);

        // Act
        IEnumerable<UserResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count());
        List<UserResponse> resultsList = [..results];
        List<UserEntity> entitiesList = [.. entities];

        for (int i = 0; i < resultsList.Count; i++)
        {
            Assert.Equal(entitiesList[i].Id, resultsList[i].Id);
            Assert.Equal(entitiesList[i].Username, resultsList[i].Username);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        List<UserEntity> entities = [];

        // Act
        IEnumerable<UserResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void ToResponse_WhenMappingWithoutUpdatedDateTime_ShouldMapCorrectly()
    {
        // Arrange
        UserEntity entity = UserEntityFixture.CreateUserEntity();
        entity.UpdatedOnUtc = null;

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.UpdatedOnUtc);
    }
}
