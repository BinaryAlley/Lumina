#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common.Pagination;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Common.Pagination;

/// <summary>
/// Contains unit tests for the <see cref="PaginationDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginationDataDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void CurrentPage_WhenNotSet_ShouldDefaultToOne()
    {
        // Arrange
        PaginationDataDto sut = new();

        // Act
        int currentPage = sut.CurrentPage;

        // Assert
        Assert.Equal(1, currentPage);
    }

    [Fact]
    public void PerPage_WhenNotSet_ShouldDefaultToTwoHundred()
    {
        // Arrange
        PaginationDataDto sut = new();

        // Act
        int perPage = sut.PerPage;

        // Assert
        Assert.Equal(200, perPage);
    }

    [Theory]
    [InlineData(0)] // zero is below the minimum
    [InlineData(-5)] // negative values are below the minimum
    public void CurrentPage_WhenSettingValueBelowOne_ShouldClampToOne(int value)
    {
        // Arrange
        PaginationDataDto sut = new();

        // Act
        sut.CurrentPage = value;

        // Assert
        Assert.Equal(1, sut.CurrentPage);
    }

    [Theory]
    [InlineData(0)] // zero is below the minimum
    [InlineData(-10)] // negative values are below the minimum
    public void PerPage_WhenSettingValueBelowOne_ShouldClampToOne(int value)
    {
        // Arrange
        PaginationDataDto sut = new();

        // Act
        sut.PerPage = value;

        // Assert
        Assert.Equal(1, sut.PerPage);
    }

    [Fact]
    public void CurrentPage_WhenSettingValueAboveOne_ShouldPreserveValue()
    {
        // Arrange
        PaginationDataDto sut = new();

        // Act
        sut.CurrentPage = 5;

        // Assert
        Assert.Equal(5, sut.CurrentPage);
    }

    [Fact]
    public void PerPage_WhenSettingValueAboveOne_ShouldPreserveValue()
    {
        // Arrange
        PaginationDataDto sut = new();

        // Act
        sut.PerPage = 25;

        // Assert
        Assert.Equal(25, sut.PerPage);
    }

    [Fact]
    public void RoundTrip_WhenSerializingValidPaginationData_ShouldPreserveValues()
    {
        // Arrange
        PaginationDataDto expected = new() { CurrentPage = 3, PerPage = 50 };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PaginationDataDto? actual = JsonSerializer.Deserialize<PaginationDataDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.CurrentPage, actual.CurrentPage);
        Assert.Equal(expected.PerPage, actual.PerPage);
    }
}
