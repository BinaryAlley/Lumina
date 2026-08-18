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
/// Contains unit tests for the <see cref="BookRatingDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingDtoTests
{
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidBookRatingDto()
    {
        // Act
        BookRatingDto sut = _bookRatingDtoFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.True(sut.Value.HasValue);
        Assert.True(sut.MaxValue.HasValue);
    }

    [Fact]
    public void Create_WhenOmittingOptionalProperties_ShouldReturnNullSourceAndVoteCount()
    {
        // Act
        BookRatingDto sut = _bookRatingDtoFixture.Create(includeOptionalProperties: false);

        // Assert
        Assert.Null(sut.Source);
        Assert.Null(sut.VoteCount);
    }

    [Fact]
    public void RoundTrip_WhenSerializingRating_ShouldPreserveValues()
    {
        // Arrange
        BookRatingDto expected = _bookRatingDtoFixture.Create(source: BookRatingSource.Goodreads);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        BookRatingDto? actual = JsonSerializer.Deserialize<BookRatingDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingRating_ShouldSerializeSourceAsCamelCaseString()
    {
        // Arrange
        BookRatingDto sut = _bookRatingDtoFixture.Create(source: BookRatingSource.OpenLibrary);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"Source\":\"openLibrary\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        BookRatingDto first = _bookRatingDtoFixture.Create(value: 4, maxValue: 5, source: BookRatingSource.User, voteCount: 10);
        BookRatingDto second = _bookRatingDtoFixture.Create(value: 4, maxValue: 5, source: BookRatingSource.User, voteCount: 10);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
