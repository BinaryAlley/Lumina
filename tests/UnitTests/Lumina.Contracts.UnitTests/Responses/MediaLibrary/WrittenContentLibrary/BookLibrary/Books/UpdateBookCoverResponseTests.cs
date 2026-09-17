#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCoverResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverResponseTests
{
    private readonly UpdateBookCoverResponseFixture _updateBookCoverResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUpdateBookCoverResponse_ShouldPreserveValues()
    {
        // Arrange
        UpdateBookCoverResponse expected = _updateBookCoverResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdateBookCoverResponse? actual = JsonSerializer.Deserialize<UpdateBookCoverResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        UpdateBookCoverResponse sut = _updateBookCoverResponseFixture.Create();

        // Act
        sut.Deconstruct(out string coverPath);

        // Assert
        Assert.Equal(sut.CoverPath, coverPath);
    }
}
