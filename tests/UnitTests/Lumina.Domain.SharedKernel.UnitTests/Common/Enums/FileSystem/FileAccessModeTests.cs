#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="FileAccessMode"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileAccessModeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void FileAccessMode_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        FileAccessMode[] values = Enum.GetValues<FileAccessMode>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (FileAccessMode value in Enum.GetValues<FileAccessMode>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            FileAccessMode deserialized = JsonSerializer.Deserialize<FileAccessMode>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}
