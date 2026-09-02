#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryBookReaderEnabledRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledRequestTests
{
    private readonly SetLibraryBookReaderEnabledRequestFixture _setLibraryBookReaderEnabledRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidSetLibraryBookReaderEnabledRequest()
    {
        // Act
        SetLibraryBookReaderEnabledRequest sut = _setLibraryBookReaderEnabledRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
        Assert.NotEqual(Guid.Empty, sut.PluginId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingSetLibraryBookReaderEnabledRequest_ShouldPreserveValues()
    {
        // Arrange
        SetLibraryBookReaderEnabledRequest expected = _setLibraryBookReaderEnabledRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        SetLibraryBookReaderEnabledRequest? actual = JsonSerializer.Deserialize<SetLibraryBookReaderEnabledRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
