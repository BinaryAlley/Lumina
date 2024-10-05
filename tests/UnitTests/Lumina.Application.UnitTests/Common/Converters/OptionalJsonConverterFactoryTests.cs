#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Converters;
using Lumina.Application.UnitTests.Common.Converters.Fixtures;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Application.UnitTests.Common.Converters;

/// <summary>
/// Contains unit tests for the <see cref="GetFilesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OptionalJsonConverterFactoryTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly OptionalJsonConverterFactory _factory;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionalJsonConverterFactoryTests"/> class.
    /// </summary>
    public OptionalJsonConverterFactoryTests()
    {
        _factory = new OptionalJsonConverterFactory();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void CanConvert_WhenTypeIsOptional_ShouldReturnTrue()
    {
        // Arrange
        Type typeToConvert = typeof(Optional<string>);

        // Act
        bool result = _factory.CanConvert(typeToConvert);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WhenTypeIsNotOptional_ShouldReturnFalse()
    {
        // Arrange
        Type typeToConvert = typeof(string);

        // Act
        bool result = _factory.CanConvert(typeToConvert);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvert_WhenTypeIsGenericButNotOptional_ShouldReturnFalse()
    {
        // Arrange
        Type typeToConvert = typeof(List<string>);

        // Act
        bool result = _factory.CanConvert(typeToConvert);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateConverter_WhenTypeIsOptionalString_ShouldReturnCorrectConverterType()
    {
        // Arrange
        Type typeToConvert = typeof(Optional<string>);
        JsonSerializerOptions options = new();

        // Act
        JsonConverter converter = _factory.CreateConverter(typeToConvert, options);

        // Assert
        converter.Should().BeOfType<OptionalJsonConverter<string>>();
    }

    [Fact]
    public void CreateConverter_WhenTypeIsOptionalInt_ShouldReturnCorrectConverterType()
    {
        // Arrange
        Type typeToConvert = typeof(Optional<int>);
        JsonSerializerOptions options = new();

        // Act
        JsonConverter converter = _factory.CreateConverter(typeToConvert, options);

        // Assert
        converter.Should().BeOfType<OptionalJsonConverter<int>>();
    }

    [Fact]
    public void CreateConverter_WhenTypeIsOptionalCustomClass_ShouldReturnCorrectConverterType()
    {
        // Arrange
        Type typeToConvert = typeof(Optional<OptionalJsonConverterFactoryFixture>);
        JsonSerializerOptions options = new();

        // Act
        JsonConverter converter = _factory.CreateConverter(typeToConvert, options);

        // Assert
        converter.Should().BeOfType<OptionalJsonConverter<OptionalJsonConverterFactoryFixture>>();
    }
    #endregion
}
