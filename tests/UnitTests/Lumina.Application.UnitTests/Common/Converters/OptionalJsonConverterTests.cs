#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Converters;
using Lumina.Domain.Common.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Application.UnitTests.Common.Converters;

/// <summary>
/// Contains unit tests for the <see cref="OptionalJsonConverter"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OptionalJsonConverterTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly JsonSerializerOptions _options;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionalJsonConverterTests"/> class.
    /// </summary>
    public OptionalJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new OptionalJsonConverter<string>());
        _options.Converters.Add(new OptionalJsonConverter<int>());
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Read_WhenJsonIsNull_ShouldReturnNone()
    {
        // Arrange
        string json = "null";

        // Act
        Optional<string> result = JsonSerializer.Deserialize<Optional<string>>(json, _options);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Read_WhenJsonHasStringValue_ShouldReturnSomeWithCorrectValue()
    {
        // Arrange
        string json = "\"test\"";

        // Act
        Optional<string> result = JsonSerializer.Deserialize<Optional<string>>(json, _options);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    [Fact]
    public void Read_WhenJsonHasIntValue_ShouldReturnSomeWithCorrectValue()
    {
        // Arrange
        string json = "42";

        // Act
        Optional<int> result = JsonSerializer.Deserialize<Optional<int>>(json, _options);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Write_WhenOptionalHasNoValue_ShouldWriteNull()
    {
        // Arrange
        Optional<string> optional = Optional<string>.None();

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Should().Be("null");
    }

    [Fact]
    public void Write_WhenOptionalHasStringValue_ShouldWriteCorrectJson()
    {
        // Arrange
        Optional<string> optional = Optional<string>.Some("test");

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Should().Be("\"test\"");
    }

    [Fact]
    public void Write_WhenOptionalHasIntValue_ShouldWriteCorrectJson()
    {
        // Arrange
        Optional<int> optional = Optional<int>.Some(42);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Should().Be("42");
    }

    [Fact]
    public void ReadWrite_WhenSerializingAndDeserializingSomeValue_ShouldPreserveValue()
    {
        // Arrange
        Optional<string> original = Optional<string>.Some("test");

        // Act
        string json = JsonSerializer.Serialize(original, _options);
        Optional<string> deserialized = JsonSerializer.Deserialize<Optional<string>>(json, _options);

        // Assert
        deserialized.Should().Be(original);
    }

    [Fact]
    public void ReadWrite_WhenSerializingAndDeserializingNone_ShouldPreserveNone()
    {
        // Arrange
        Optional<string> original = Optional<string>.None();

        // Act
        string json = JsonSerializer.Serialize(original, _options);
        Optional<string> deserialized = JsonSerializer.Deserialize<Optional<string>>(json, _options);

        // Assert
        deserialized.Should().Be(original);
    }
    #endregion
}
