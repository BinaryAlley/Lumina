#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;

/// <summary>
/// Contains unit tests for the <see cref="User"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserTests
{
    private readonly UserIdFixture _userIdFixture = new();
    private readonly UserFixture _userFixture = new();

    [Fact]
    public void Create_WhenCalledWithUsername_ShouldCreateUserWithGeneratedId()
    {
        // Act
        Result<User> result = User.Create("testUser");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("testUser", result.Value.Username);
        Assert.NotEqual(default, result.Value.Id.Value);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingId_ShouldCreateUserWithThatId()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<User> result = User.Create(_userIdFixture.Create(id), "testUser");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id.Value);
        Assert.Equal("testUser", result.Value.Username);
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Act
        Result<User> firstResult = User.Create("firstUser");
        Result<User> secondResult = User.Create("secondUser");

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.Id.Value, secondResult.Value.Id.Value);
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        User firstUser = _userFixture.Create(id: id);
        User secondUser = _userFixture.Create(id: id);

        // Act
        bool result = firstUser.Equals(secondUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        User firstUser = _userFixture.Create(username: "firstUser");
        User secondUser = _userFixture.Create(username: "secondUser");

        // Act
        bool result = firstUser.Equals(secondUser);

        // Assert
        Assert.False(result);
    }
}
