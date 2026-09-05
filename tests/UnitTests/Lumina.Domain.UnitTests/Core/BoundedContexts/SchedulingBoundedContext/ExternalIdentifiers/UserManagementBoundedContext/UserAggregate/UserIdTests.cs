#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;

/// <summary>
/// Contains unit tests for the <see cref="UserId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserIdTests
{
    private readonly UserIdFixture _userIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        UserId userId = UserId.CreateUnique();

        // Assert
        Assert.NotNull(userId);
        Assert.NotEqual(default, userId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        UserId userId = UserId.Create(value);

        // Assert
        Assert.Equal(value, userId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        UserId firstId = _userIdFixture.Create(value);
        UserId secondId = _userIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        UserId firstId = _userIdFixture.Create();
        UserId secondId = _userIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
