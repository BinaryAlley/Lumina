#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Common.Primitives;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Primitives;

/// <summary>
/// Contains unit tests for the <see cref="Optional{T}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OptionalTests
{
    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Some_WhenCreatingOptionalWithValue_ShouldHaveValueAndCorrectValue()
    {
        // Arrange
        int value = 42;

        // Act
        Optional<int> optional = Optional<int>.Some(value);

        // Assert
        optional.HasValue.Should().BeTrue();
        optional.Value.Should().Be(value);
    }

    [Fact]
    public void None_WhenCreatingOptionalWithoutValue_ShouldNotHaveValueAndHaveDefaultValue()
    {
        // Arrange & Act
        Optional<int> optional = Optional<int>.None();

        // Assert
        optional.HasValue.Should().BeFalse();
        optional.Value.Should().Be(default);
    }

    [Fact]
    public void ImplicitConversion_WhenConvertingValueToOptional_ShouldCreateOptionalWithValue()
    {
        // Arrange
        string value = "test";

        // Act
        Optional<string> optional = value;

        // Assert
        optional.HasValue.Should().BeTrue();
        optional.Value.Should().Be(value);
    }

    [Fact]
    public void FromNullable_WhenCreatingFromNonNullReferenceType_ShouldCreateOptionalWithValue()
    {
        // Arrange
        string? nullableValue = "test";

        // Act
        Optional<string> optional = Optional<string>.FromNullable(nullableValue);

        // Assert
        optional.HasValue.Should().BeTrue();
        optional.Value.Should().Be(nullableValue);
    }

    [Fact]
    public void FromNullable_WhenCreatingFromNullReferenceType_ShouldCreateOptionalWithoutValue()
    {
        // Arrange
        string? nullableValue = null;

        // Act
        Optional<string> optional = Optional<string>.FromNullable(nullableValue);

        // Assert
        optional.HasValue.Should().BeFalse();
        optional.Value.Should().BeNull();
    }

    [Fact]
    public void FromNullable_WhenCreatingFromNonNullValueType_ShouldCreateOptionalWithValue()
    {
        // Arrange
        int? nullableValue = 42;

        // Act
        Optional<int> optional = Optional<int>.FromNullable(nullableValue);

        // Assert
        optional.HasValue.Should().BeTrue();
        optional.Value.Should().Be(nullableValue.Value);
    }

    [Fact]
    public void FromNullable_WhenCreatingFromNullValueType_ShouldCreateOptionalWithoutValue()
    {
        // Arrange
        int? nullableValue = null;

        // Act
        Optional<int> optional = Optional<int>.FromNullable(nullableValue);

        // Assert
        optional.HasValue.Should().BeFalse();
        optional.Value.Should().Be(default);
    }

    [Fact]
    public void ToString_WhenOptionalHasValue_ShouldReturnValueToString()
    {
        // Arrange
        Optional<int> optional = Optional<int>.Some(42);

        // Act
        string result = optional.ToString();

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void ToString_WhenOptionalHasNoValue_ShouldReturnEmptyString()
    {
        // Arrange
        Optional<int> optional = Optional<int>.None();

        // Act
        string result = optional.ToString();

        // Assert
        result.Should().BeEmpty();
    }
    #endregion
}
