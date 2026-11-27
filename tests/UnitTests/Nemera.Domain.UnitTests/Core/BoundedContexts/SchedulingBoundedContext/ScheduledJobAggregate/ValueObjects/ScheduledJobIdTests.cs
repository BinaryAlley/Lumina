#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobIdTests
{
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        ScheduledJobId scheduledJobId = ScheduledJobId.CreateUnique();

        // Assert
        Assert.NotNull(scheduledJobId);
        Assert.NotEqual(default, scheduledJobId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        ScheduledJobId scheduledJobId = ScheduledJobId.Create(value);

        // Assert
        Assert.Equal(value, scheduledJobId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        ScheduledJobId firstId = _scheduledJobIdFixture.Create(value);
        ScheduledJobId secondId = _scheduledJobIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        ScheduledJobId firstId = _scheduledJobIdFixture.Create();
        ScheduledJobId secondId = _scheduledJobIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
