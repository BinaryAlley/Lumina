#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Common;

/// <summary>
/// Contains unit tests for the <see cref="PaginatedResponse{TData}"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginatedResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void PaginatedResponse_WhenConstructed_ShouldRequireAllProperties()
    {
        // Act
        PaginatedResponse<int> sut = new()
        {
            Data = [1, 2, 3],
            CurrentPage = 1,
            PerPage = 10,
            Count = 3,
            NumberOfPages = 1
        };

        // Assert
        Assert.Equal([1, 2, 3], sut.Data);
        Assert.Equal(1, sut.CurrentPage);
        Assert.Equal(10, sut.PerPage);
        Assert.Equal(3, sut.Count);
        Assert.Equal(1, sut.NumberOfPages);
    }

    [Fact]
    public void RoundTrip_WhenSerializingPaginatedResponse_ShouldPreserveValues()
    {
        // Arrange
        PaginatedResponse<string> expected = new()
        {
            Data = ["book1", "book2"],
            CurrentPage = 2,
            PerPage = 20,
            Count = 2,
            NumberOfPages = 1
        };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PaginatedResponse<string>? actual = JsonSerializer.Deserialize<PaginatedResponse<string>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Data, actual.Data);
        Assert.Equal(expected.CurrentPage, actual.CurrentPage);
        Assert.Equal(expected.PerPage, actual.PerPage);
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected.NumberOfPages, actual.NumberOfPages);
    }

    [Fact]
    public void Deserialize_WhenDataIsMissing_ShouldThrowJsonException()
    {
        // Arrange
        string json = """{ "currentPage": 1, "perPage": 10, "count": 0, "numberOfPages": 0 }""";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PaginatedResponse<int>>(json, _jsonOptions));
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        PaginatedResponse<int> first = new()
        {
            Data = [1, 2, 3],
            CurrentPage = 1,
            PerPage = 10,
            Count = 3,
            NumberOfPages = 1
        };
        PaginatedResponse<int> second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
