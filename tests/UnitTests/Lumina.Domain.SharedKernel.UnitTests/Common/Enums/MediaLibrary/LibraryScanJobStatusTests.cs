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
/// Contains unit tests for the <see cref="LibraryScanJobStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanJobStatusTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void LibraryScanJobStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        LibraryScanJobStatus[] values = Enum.GetValues<LibraryScanJobStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (LibraryScanJobStatus value in Enum.GetValues<LibraryScanJobStatus>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            LibraryScanJobStatus deserialized = JsonSerializer.Deserialize<LibraryScanJobStatus>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}
