#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Contracts.Responses.UsersManagement.Users;
using System;
using System.Collections.Generic;
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
        UserEntity entity = UserEntityFixture.CreateUserEntity();

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Username.Should().Be(entity.Username);
        result.CreatedOnUtc.Should().Be(entity.CreatedOnUtc);
        result.UpdatedOnUtc.Should().Be(entity.UpdatedOnUtc);
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
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
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
        result.Should().NotBeNull();
        result.UpdatedOnUtc.Should().Be(entity.UpdatedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenMappingWithLibraries_ShouldNotAffectMapping()
    {
        // Arrange
        UserEntity entity = UserEntityFixture.CreateUserEntity(2);

        // Act
        UserResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Username.Should().Be(entity.Username);
        result.CreatedOnUtc.Should().Be(entity.CreatedOnUtc);
        result.UpdatedOnUtc.Should().Be(entity.UpdatedOnUtc);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleEntities_ShouldMapCorrectly()
    {
        // Arrange
        List<UserEntity> entities = UserEntityFixture.CreateMany(3);

        // Act
        IEnumerable<UserResponse> results = entities.ToResponses();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results.Should().BeEquivalentTo(entities, options => options
            .Including(x => x.Id)
            .Including(x => x.Username));
    }

    [Fact]
    public void ToResponses_WhenMappingEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        List<UserEntity> entities = [];

        // Act
        IEnumerable<UserResponse> results = entities.ToResponses();

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
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
        result.Should().NotBeNull();
        result.UpdatedOnUtc.Should().BeNull();
    }
}
