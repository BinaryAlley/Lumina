#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="LibraryBookReaderResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryBookReaderResponseTests
{
    private readonly LibraryBookReaderResponseFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingLibraryBookReaderResponse_ShouldPreserveValues()
    {
        // Arrange
        LibraryBookReaderResponse expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LibraryBookReaderResponse? actual = JsonSerializer.Deserialize<LibraryBookReaderResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
}
