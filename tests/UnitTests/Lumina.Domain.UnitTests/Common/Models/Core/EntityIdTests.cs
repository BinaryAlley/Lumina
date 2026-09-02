#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Models.Core;

/// <summary>
/// Contains unit tests for the <see cref="EntityId{TId}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EntityIdTests
{
    [Fact]
    public void Value_WhenConstructed_ShouldBeSet()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        TestEntityId entityId = new(value);

        // Assert
        Assert.Equal(value, entityId.Value);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnValueAsString()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        TestEntityId entityId = new(value);

        // Act
        string? result = entityId.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        TestEntityId firstId = new(value);
        TestEntityId secondId = new(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        TestEntityId firstId = new(Guid.NewGuid());
        TestEntityId secondId = new(Guid.NewGuid());

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        TestEntityId firstId = new(value);
        TestEntityId secondId = new(value);

        // Act
        int firstHashCode = firstId.GetHashCode();
        int secondHashCode = secondId.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void GetEqualityComponents_WhenNotOverridden_ShouldYieldOnlyTheValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        DefaultEntityId entityId = new(value);

        // Act
        IEnumerable<object?> components = entityId.GetEqualityComponents();

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(value, component);
    }

    [Fact]
    public void Constructor_WhenCalledWithoutArguments_ShouldLeaveValueAsDefault()
    {
        // Act
        DefaultEntityId entityId = new();

        // Assert
        Assert.Equal(default, entityId.Value);
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="EntityId{TId}"/> class.
    /// </summary>
    private sealed class TestEntityId : EntityId<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEntityId"/> class.
        /// </summary>
        /// <param name="value">The value representing this object.</param>
        public TestEntityId(Guid value) : base(value)
        {
        }

        /// <summary>
        /// Gets the list of items that define equality of the object.
        /// </summary>
        /// <returns>A list of items defining the equality.</returns>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="EntityId{TId}"/> class that relies on the base equality components.
    /// </summary>
    private sealed class DefaultEntityId : EntityId<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEntityId"/> class.
        /// </summary>
        /// <param name="value">The value representing this object.</param>
        public DefaultEntityId(Guid value) : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEntityId"/> class.
        /// </summary>
        public DefaultEntityId() : base()
        {
        }
    }
}
