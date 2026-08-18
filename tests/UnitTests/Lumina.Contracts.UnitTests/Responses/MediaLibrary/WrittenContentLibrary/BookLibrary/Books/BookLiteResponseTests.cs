#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookLiteResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLiteResponseTests
{
    private readonly BookLiteResponseFixture _bookLiteResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingBookLiteResponse_ShouldPreserveValues()
    {
        // Arrange
        BookLiteResponse expected = _bookLiteResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        BookLiteResponse? actual = JsonSerializer.Deserialize<BookLiteResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingBookLiteResponseWithNullValues_ShouldPreserveNulls()
    {
        // Arrange
        BookLiteResponse expected = _bookLiteResponseFixture.Create() with { ReleaseYear = null, CoverPath = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        BookLiteResponse? actual = JsonSerializer.Deserialize<BookLiteResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.ReleaseYear);
        Assert.Null(actual.CoverPath);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        BookLiteResponse sut = _bookLiteResponseFixture.Create();

        // Act
        (Guid sutId, string title, int? releaseYear, string? coverPath) = sut;

        // Assert
        Assert.Equal(sut.Id, sutId);
        Assert.Equal(sut.Title, title);
        Assert.Equal(sut.ReleaseYear, releaseYear);
        Assert.Equal(sut.CoverPath, coverPath);
    }
}
