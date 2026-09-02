#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="DeleteLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryRequestTests
{
    private readonly DeleteLibraryRequestFixture _deleteLibraryRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidDeleteLibraryRequest()
    {
        // Act
        DeleteLibraryRequest sut = _deleteLibraryRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.Id);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        DeleteLibraryRequest first = _deleteLibraryRequestFixture.Create();
        DeleteLibraryRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingDeleteLibraryRequest_ShouldPreserveValues()
    {
        // Arrange
        DeleteLibraryRequest expected = _deleteLibraryRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        DeleteLibraryRequest? actual = JsonSerializer.Deserialize<DeleteLibraryRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
