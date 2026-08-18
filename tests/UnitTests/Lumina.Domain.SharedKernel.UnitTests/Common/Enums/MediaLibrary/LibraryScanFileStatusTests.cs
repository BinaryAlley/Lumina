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
/// Contains unit tests for the <see cref="LibraryScanFileStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFileStatusTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void LibraryScanFileStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        LibraryScanFileStatus[] values = Enum.GetValues<LibraryScanFileStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (LibraryScanFileStatus value in Enum.GetValues<LibraryScanFileStatus>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            LibraryScanFileStatus deserialized = JsonSerializer.Deserialize<LibraryScanFileStatus>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}
