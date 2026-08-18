#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="IsbnDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnDtoTests
{
    private readonly IsbnDtoFixture _isbnDtoFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidIsbnDto()
    {
        // Act
        IsbnDto sut = _isbnDtoFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Value));
    }

    [Fact]
    public void Create_WhenFormatIsIsbn10_ShouldReturnIsbn10Value()
    {
        // Act
        IsbnDto sut = _isbnDtoFixture.Create(format: IsbnFormat.Isbn10);

        // Assert
        Assert.Equal(IsbnFormat.Isbn10, sut.Format);
        Assert.Contains("-", sut.Value, StringComparison.Ordinal);
    }

    [Fact]
    public void RoundTrip_WhenSerializingIsbn_ShouldPreserveValues()
    {
        // Arrange
        IsbnDto expected = _isbnDtoFixture.Create(format: IsbnFormat.Isbn13);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        IsbnDto? actual = JsonSerializer.Deserialize<IsbnDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingIsbn_ShouldSerializeFormatAsCamelCaseString()
    {
        // Arrange
        IsbnDto sut = _isbnDtoFixture.Create(format: IsbnFormat.Isbn13);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"Format\":\"isbn13\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        IsbnDto first = _isbnDtoFixture.Create(value: "978-0-306-40615-7", format: IsbnFormat.Isbn13);
        IsbnDto second = _isbnDtoFixture.Create(value: "978-0-306-40615-7", format: IsbnFormat.Isbn13);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
