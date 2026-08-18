#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathRequestTests
{
    private readonly SplitPathRequestFixture _splitPathRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidSplitPathRequest()
    {
        // Act
        SplitPathRequest sut = _splitPathRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingSplitPathRequest_ShouldPreserveValues()
    {
        // Arrange
        SplitPathRequest expected = _splitPathRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        SplitPathRequest? actual = JsonSerializer.Deserialize<SplitPathRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        SplitPathRequest first = _splitPathRequestFixture.Create(path: @"C:\Media\Books");
        SplitPathRequest second = _splitPathRequestFixture.Create(path: @"C:\Media\Books");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
