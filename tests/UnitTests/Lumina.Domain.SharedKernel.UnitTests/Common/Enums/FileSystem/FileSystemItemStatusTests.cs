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
/// Contains unit tests for the <see cref="FileSystemItemStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemStatusTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void FileSystemItemStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        FileSystemItemStatus[] values = Enum.GetValues<FileSystemItemStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (FileSystemItemStatus value in Enum.GetValues<FileSystemItemStatus>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            FileSystemItemStatus deserialized = JsonSerializer.Deserialize<FileSystemItemStatus>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}
