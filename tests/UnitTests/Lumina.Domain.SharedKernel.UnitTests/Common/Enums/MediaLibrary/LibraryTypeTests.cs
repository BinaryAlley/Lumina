#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="LibraryType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void LibraryType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        LibraryType[] values = Enum.GetValues<LibraryType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void Book_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)LibraryType.Book;

        // Assert
        Assert.Equal(0, value);
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (LibraryType value in Enum.GetValues<LibraryType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            LibraryType deserialized = JsonSerializer.Deserialize<LibraryType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}
