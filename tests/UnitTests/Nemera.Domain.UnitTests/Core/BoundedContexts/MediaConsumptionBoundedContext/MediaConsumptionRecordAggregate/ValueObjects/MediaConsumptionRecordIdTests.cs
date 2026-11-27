#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionRecordId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordIdTests
{
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        MediaConsumptionRecordId mediaConsumptionRecordId = MediaConsumptionRecordId.CreateUnique();

        // Assert
        Assert.NotNull(mediaConsumptionRecordId);
        Assert.NotEqual(default, mediaConsumptionRecordId.Value);
    }

    [Fact]
    public void CreateUnique_WhenCalledTwice_ShouldReturnDistinctIds()
    {
        // Act
        MediaConsumptionRecordId firstId = MediaConsumptionRecordId.CreateUnique();
        MediaConsumptionRecordId secondId = MediaConsumptionRecordId.CreateUnique();

        // Assert
        Assert.NotEqual(firstId.Value, secondId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        MediaConsumptionRecordId mediaConsumptionRecordId = MediaConsumptionRecordId.Create(value);

        // Assert
        Assert.Equal(value, mediaConsumptionRecordId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaConsumptionRecordId firstId = _mediaConsumptionRecordIdFixture.Create(value);
        MediaConsumptionRecordId secondId = _mediaConsumptionRecordIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        MediaConsumptionRecordId firstId = _mediaConsumptionRecordIdFixture.Create();
        MediaConsumptionRecordId secondId = _mediaConsumptionRecordIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WhenValuesAreEqual_ShouldReturnSameHashCode()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaConsumptionRecordId firstId = _mediaConsumptionRecordIdFixture.Create(value);
        MediaConsumptionRecordId secondId = _mediaConsumptionRecordIdFixture.Create(value);

        // Act
        int firstHashCode = firstId.GetHashCode();
        int secondHashCode = secondId.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void GetEqualityComponents_WhenCalled_ShouldYieldTheGuidValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaConsumptionRecordId mediaConsumptionRecordId = _mediaConsumptionRecordIdFixture.Create(value);

        // Act
        object?[] components = [.. mediaConsumptionRecordId.GetEqualityComponents()];

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(value, Assert.IsType<Guid>(component));
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnTheGuidString()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaConsumptionRecordId mediaConsumptionRecordId = _mediaConsumptionRecordIdFixture.Create(value);

        // Act
        string? result = mediaConsumptionRecordId.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }
}
