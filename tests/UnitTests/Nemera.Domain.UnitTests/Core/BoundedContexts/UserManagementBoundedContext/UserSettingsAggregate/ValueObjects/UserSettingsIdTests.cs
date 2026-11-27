#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsIdTests
{
    private readonly UserSettingsIdFixture _userSettingsIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        UserSettingsId userSettingsId = UserSettingsId.CreateUnique();

        // Assert
        Assert.NotNull(userSettingsId);
        Assert.NotEqual(default, userSettingsId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        UserSettingsId userSettingsId = UserSettingsId.Create(value);

        // Assert
        Assert.Equal(value, userSettingsId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        UserSettingsId firstId = _userSettingsIdFixture.Create(value);
        UserSettingsId secondId = _userSettingsIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        UserSettingsId firstId = _userSettingsIdFixture.Create(value);
        UserSettingsId secondId = _userSettingsIdFixture.Create(value);

        // Act
        int firstHashCode = firstId.GetHashCode();
        int secondHashCode = secondId.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        UserSettingsId firstId = _userSettingsIdFixture.Create();
        UserSettingsId secondId = _userSettingsIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
