#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksLiteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteRequestTests
{
    private readonly GetBooksLiteRequestFixture _getBooksLiteRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetBooksLiteRequest()
    {
        // Act
        GetBooksLiteRequest sut = _getBooksLiteRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
        Assert.True(sut.CurrentPage.HasValue);
        Assert.True(sut.PerPage.HasValue);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetBooksLiteRequest_ShouldPreserveValues()
    {
        // Arrange
        GetBooksLiteRequest expected = _getBooksLiteRequestFixture.Create(sortOrder: SortOrder.Descending);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetBooksLiteRequest? actual = JsonSerializer.Deserialize<GetBooksLiteRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingGetBooksLiteRequest_ShouldSerializeSortOrderAsCamelCaseString()
    {
        // Arrange
        GetBooksLiteRequest sut = _getBooksLiteRequestFixture.Create(sortOrder: SortOrder.Ascending);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"SortOrder\":\"ascending\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        GetBooksLiteRequest sut = _getBooksLiteRequestFixture.Create();

        // Act
        (Guid libraryId, int? currentPage, int? perPage, string? searchTerm, string? filterAlphaKey, bool ignoreThePrefixForAlphaPicker, string? sortBy, SortOrder? sortOrder) = sut;

        // Assert
        Assert.Equal(sut.LibraryId, libraryId);
        Assert.Equal(sut.CurrentPage, currentPage);
        Assert.Equal(sut.PerPage, perPage);
        Assert.Equal(sut.SearchTerm, searchTerm);
        Assert.Equal(sut.FilterAlphaKey, filterAlphaKey);
        Assert.Equal(sut.IgnoreThePrefixForAlphaPicker, ignoreThePrefixForAlphaPicker);
        Assert.Equal(sut.SortBy, sortBy);
        Assert.Equal(sut.SortOrder, sortOrder);
    }
}
