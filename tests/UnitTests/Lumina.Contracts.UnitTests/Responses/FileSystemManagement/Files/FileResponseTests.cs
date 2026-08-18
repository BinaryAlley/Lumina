#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="FileResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileResponseTests
{
    private readonly FileResponseFixture _fileResponseFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidFileResponse()
    {
        // Act
        FileResponse sut = _fileResponseFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
        Assert.False(string.IsNullOrWhiteSpace(sut.Name));
    }

    [Fact]
    public void RoundTrip_WhenSerializingFileResponse_ShouldPreserveValues()
    {
        // Arrange
        FileResponse expected = _fileResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        FileResponse? actual = JsonSerializer.Deserialize<FileResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        FileResponse sut = _fileResponseFixture.Create();

        // Act
        (string path, string name, System.DateTime dateCreated, System.DateTime dateModified, long size) = sut;

        // Assert
        Assert.Equal(sut.Path, path);
        Assert.Equal(sut.Name, name);
        Assert.Equal(sut.DateCreated, dateCreated);
        Assert.Equal(sut.DateModified, dateModified);
        Assert.Equal(sut.Size, size);
    }
}
