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
/// Contains unit tests for the <see cref="GetBooksRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksRequestTests
{
    private readonly GetBooksRequestFixture _getBooksRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetBooksRequest()
    {
        // Act
        GetBooksRequest sut = _getBooksRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
        Assert.True(sut.CurrentPage.HasValue);
        Assert.True(sut.PerPage.HasValue);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetBooksRequest_ShouldPreserveValues()
    {
        // Arrange
        GetBooksRequest expected = _getBooksRequestFixture.Create(sortOrder: SortOrder.Ascending);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetBooksRequest? actual = JsonSerializer.Deserialize<GetBooksRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingGetBooksRequest_ShouldSerializeSortOrderAsCamelCaseString()
    {
        // Arrange
        GetBooksRequest sut = _getBooksRequestFixture.Create(sortOrder: SortOrder.Descending);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"SortOrder\":\"descending\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        GetBooksRequest sut = _getBooksRequestFixture.Create();

        // Act
        (Guid libraryId, int? currentPage, int? perPage, string? searchTerm, string? sortBy, SortOrder? sortOrder) = sut;

        // Assert
        Assert.Equal(sut.LibraryId, libraryId);
        Assert.Equal(sut.CurrentPage, currentPage);
        Assert.Equal(sut.PerPage, perPage);
        Assert.Equal(sut.SearchTerm, searchTerm);
        Assert.Equal(sut.SortBy, sortBy);
        Assert.Equal(sut.SortOrder, sortOrder);
    }
}
