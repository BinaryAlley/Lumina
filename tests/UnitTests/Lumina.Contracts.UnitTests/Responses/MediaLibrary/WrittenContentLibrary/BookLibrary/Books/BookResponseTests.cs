#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookResponseTests
{
    private readonly BookResponseFixture _bookResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingBookResponse_ShouldPreserveValues()
    {
        // Arrange
        BookResponse expected = _bookResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        BookResponse? actual = JsonSerializer.Deserialize<BookResponse>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingBookResponse_ShouldSerializeEnumsAsCamelCaseStrings()
    {
        // Arrange
        BookResponse sut = _bookResponseFixture.Create(
            format: BookFormat.Paperback,
            metadataStatus: MetadataStatus.Pending);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"Format\":\"paperback\"", json, StringComparison.Ordinal);
        Assert.Contains("\"MetadataStatus\":\"pending\"", json, StringComparison.Ordinal);
    }
}
