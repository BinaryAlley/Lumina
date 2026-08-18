#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemTypeResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTypeResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingFileSystemTypeResponse_ShouldPreserveValues()
    {
        // Arrange
        FileSystemTypeResponse expected = new(PlatformType.Windows);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        FileSystemTypeResponse? actual = JsonSerializer.Deserialize<FileSystemTypeResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingFileSystemTypeResponse_ShouldSerializePlatformTypeAsCamelCaseString()
    {
        // Arrange
        FileSystemTypeResponse sut = new(PlatformType.Unix);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"PlatformType\":\"unix\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        FileSystemTypeResponse first = new(PlatformType.Windows);
        FileSystemTypeResponse second = new(PlatformType.Windows);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
