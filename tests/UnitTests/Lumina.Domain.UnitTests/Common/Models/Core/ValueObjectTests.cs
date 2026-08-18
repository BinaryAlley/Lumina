#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Models.Core;

/// <summary>
/// Contains unit tests for the <see cref="ValueObject"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValueObjectTests
{
    [Fact]
    public void Equals_WithSameComponents_ShouldReturnTrue()
    {
        // Arrange
        TestValueObject firstValueObject = new("value");
        TestValueObject secondValueObject = new("value");

        // Act
        bool result = firstValueObject.Equals(secondValueObject);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentComponents_ShouldReturnFalse()
    {
        // Arrange
        TestValueObject firstValueObject = new("first");
        TestValueObject secondValueObject = new("second");

        // Act
        bool result = firstValueObject.Equals(secondValueObject);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        TestValueObject valueObject = new("value");
        OtherTestValueObject otherValueObject = new("value");

        // Act
        bool result = valueObject.Equals(otherValueObject);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        TestValueObject valueObject = new("value");

        // Act
        bool result = valueObject.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualityOperator_WithSameComponents_ShouldReturnTrue()
    {
        // Arrange
        TestValueObject firstValueObject = new("value");
        TestValueObject secondValueObject = new("value");

        // Act
        bool result = firstValueObject == secondValueObject;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_WithDifferentComponents_ShouldReturnTrue()
    {
        // Arrange
        TestValueObject firstValueObject = new("first");
        TestValueObject secondValueObject = new("second");

        // Act
        bool result = firstValueObject != secondValueObject;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_WithSameComponents_ShouldReturnSameHashCode()
    {
        // Arrange
        TestValueObject firstValueObject = new("value");
        TestValueObject secondValueObject = new("value");

        // Act
        int firstHashCode = firstValueObject.GetHashCode();
        int secondHashCode = secondValueObject.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="ValueObject"/> class.
    /// </summary>
    private sealed class TestValueObject : ValueObject
    {
        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestValueObject"/> class.
        /// </summary>
        /// <param name="value">The value of the value object.</param>
        public TestValueObject(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the list of items that define equality of the object.
        /// </summary>
        /// <returns>A list of items defining the equality.</returns>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _value;
        }
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="ValueObject"/> class, with a different type.
    /// </summary>
    private sealed class OtherTestValueObject : ValueObject
    {
        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="OtherTestValueObject"/> class.
        /// </summary>
        /// <param name="value">The value of the value object.</param>
        public OtherTestValueObject(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the list of items that define equality of the object.
        /// </summary>
        /// <returns>A list of items defining the equality.</returns>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _value;
        }
    }
}
