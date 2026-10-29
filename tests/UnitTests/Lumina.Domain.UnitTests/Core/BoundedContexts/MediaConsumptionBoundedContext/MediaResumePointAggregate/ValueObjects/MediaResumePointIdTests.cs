#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaResumePointId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointIdTests
{
    private readonly MediaResumePointIdFixture _mediaResumePointIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        MediaResumePointId mediaResumePointId = MediaResumePointId.CreateUnique();

        // Assert
        Assert.NotNull(mediaResumePointId);
        Assert.NotEqual(default, mediaResumePointId.Value);
    }

    [Fact]
    public void CreateUnique_WhenCalledTwice_ShouldReturnDistinctIds()
    {
        // Act
        MediaResumePointId firstId = MediaResumePointId.CreateUnique();
        MediaResumePointId secondId = MediaResumePointId.CreateUnique();

        // Assert
        Assert.NotEqual(firstId.Value, secondId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        MediaResumePointId mediaResumePointId = MediaResumePointId.Create(value);

        // Assert
        Assert.Equal(value, mediaResumePointId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaResumePointId firstId = _mediaResumePointIdFixture.Create(value);
        MediaResumePointId secondId = _mediaResumePointIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        MediaResumePointId firstId = _mediaResumePointIdFixture.Create();
        MediaResumePointId secondId = _mediaResumePointIdFixture.Create();

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
        MediaResumePointId firstId = _mediaResumePointIdFixture.Create(value);
        MediaResumePointId secondId = _mediaResumePointIdFixture.Create(value);

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
        MediaResumePointId mediaResumePointId = _mediaResumePointIdFixture.Create(value);

        // Act
        object?[] components = [.. mediaResumePointId.GetEqualityComponents()];

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(value, Assert.IsType<Guid>(component));
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnTheGuidString()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaResumePointId mediaResumePointId = _mediaResumePointIdFixture.Create(value);

        // Act
        string? result = mediaResumePointId.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }
}
