#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
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
    private readonly UserEntityFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserEntityMappingTests"/> class.
    /// </summary>
    public UserEntityMappingTests()
    {
        _fixture = new UserEntityFixture();
    }

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
        result.Created.Should().Be(entity.CreatedOnUtc);
        result.Updated.Should().Be(entity.UpdatedOnUtc);
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
        result.Updated.Should().Be(entity.UpdatedOnUtc);
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
        result.Created.Should().Be(entity.CreatedOnUtc);
        result.Updated.Should().Be(entity.UpdatedOnUtc);
    }
}
