#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="CombinePathRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathRequestTests
{
    private readonly CombinePathRequestFixture _combinePathRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidCombinePathRequest()
    {
        // Act
        CombinePathRequest sut = _combinePathRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.OriginalPath));
        Assert.False(string.IsNullOrWhiteSpace(sut.NewPath));
    }

    [Fact]
    public void RoundTrip_WhenSerializingCombinePathRequest_ShouldPreserveValues()
    {
        // Arrange
        CombinePathRequest expected = _combinePathRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        CombinePathRequest? actual = JsonSerializer.Deserialize<CombinePathRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        CombinePathRequest sut = _combinePathRequestFixture.Create(originalPath: @"C:\Media", newPath: "books");

        // Act
        (string? originalPath, string? newPath) = sut;

        // Assert
        Assert.Equal(sut.OriginalPath, originalPath);
        Assert.Equal(sut.NewPath, newPath);
    }
}
