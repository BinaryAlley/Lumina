#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathRequestTests
{
    private readonly ValidatePathRequestFixture _validatePathRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidValidatePathRequest()
    {
        // Act
        ValidatePathRequest sut = _validatePathRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingValidatePathRequest_ShouldPreserveValues()
    {
        // Arrange
        ValidatePathRequest expected = _validatePathRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ValidatePathRequest? actual = JsonSerializer.Deserialize<ValidatePathRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        ValidatePathRequest first = _validatePathRequestFixture.Create(path: @"C:\Media\Books");
        ValidatePathRequest second = _validatePathRequestFixture.Create(path: @"C:\Media\Books");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
