#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookRequestTests
{
    private readonly UpdateBookRequestFixture _updateBookRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidUpdateBookRequest()
    {
        // Act
        UpdateBookRequest sut = _updateBookRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Id));
        Assert.NotNull(sut.Metadata);
        Assert.NotNull(sut.ISBNs);
        Assert.NotEmpty(sut.ISBNs);
        Assert.NotNull(sut.Contributors);
        Assert.NotEmpty(sut.Contributors);
        Assert.NotNull(sut.Ratings);
        Assert.NotEmpty(sut.Ratings);
    }

    [Fact]
    public void RoundTrip_WhenSerializingUpdateBookRequest_ShouldPreserveValues()
    {
        // Arrange
        UpdateBookRequest expected = _updateBookRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdateBookRequest? actual = JsonSerializer.Deserialize<UpdateBookRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingUpdateBookRequest_ShouldSerializeFormatAsCamelCaseString()
    {
        // Arrange
        UpdateBookRequest sut = _updateBookRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"Format\":\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Format\":\"0\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        UpdateBookRequest sut = _updateBookRequestFixture.Create();

        // Act
        (string? id, WrittenContentMetadataDto? metadata, BookFormat? format, string? edition, float? volumeNumber, BookSeriesDto? series, string? asin, string? goodreadsId, string? lccn, string? oclcNumber, string? openLibraryId, string? libraryThingId, string? googleBooksId, string? barnesAndNobleId, string? appleBooksId, List<IsbnDto>? isbns, List<MediaContributorDto>? contributors, List<BookRatingDto>? ratings) = sut;

        // Assert
        Assert.Equal(sut.Id, id);
        Assert.Equal(sut.Metadata, metadata);
        Assert.Equal(sut.Format, format);
        Assert.Equal(sut.Edition, edition);
        Assert.Equal(sut.VolumeNumber, volumeNumber);
        Assert.Equal(sut.Series, series);
        Assert.Equal(sut.ASIN, asin);
        Assert.Equal(sut.GoodreadsId, goodreadsId);
        Assert.Equal(sut.LCCN, lccn);
        Assert.Equal(sut.OCLCNumber, oclcNumber);
        Assert.Equal(sut.OpenLibraryId, openLibraryId);
        Assert.Equal(sut.LibraryThingId, libraryThingId);
        Assert.Equal(sut.GoogleBooksId, googleBooksId);
        Assert.Equal(sut.BarnesAndNobleId, barnesAndNobleId);
        Assert.Equal(sut.AppleBooksId, appleBooksId);
        Assert.Equal(sut.ISBNs, isbns);
        Assert.Equal(sut.Contributors, contributors);
        Assert.Equal(sut.Ratings, ratings);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        UpdateBookRequest first = _updateBookRequestFixture.Create();
        UpdateBookRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
