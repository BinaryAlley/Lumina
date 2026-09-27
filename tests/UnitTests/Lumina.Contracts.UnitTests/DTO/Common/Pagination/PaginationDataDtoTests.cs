#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common.Pagination;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Common.Pagination;

/// <summary>
/// Contains unit tests for the <see cref="PaginationDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginationDataDtoTests
{
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
}
