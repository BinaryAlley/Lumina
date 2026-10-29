#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetMostConsumed;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetMostConsumedRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedRequestMappingTests
{
    private readonly GetMostConsumedRequestFixture _getMostConsumedRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedRequestMappingTests"/> class.
    /// </summary>
    public GetMostConsumedRequestMappingTests()
    {
        _getMostConsumedRequestFixture = new GetMostConsumedRequestFixture();
    }

    [Fact]
    public void ToQuery_WhenMappingPopulatedRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2025-01-02T03:04:05Z";
        string toUtc = "2025-02-03T04:05:06Z";
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.Movie),
            currentPage: 3,
            perPage: 25,
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        GetMostConsumedQuery result = request.ToQuery(targetUserId.ToString());

        // Assert
        Assert.Equal(targetUserId.ToString(), result.TargetUserId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.NotNull(result.MediaKind);
        Assert.Equal(nameof(MediaKind.Movie), result.MediaKind);
        Assert.NotNull(result.FromUtc);
        Assert.Equal(fromUtc, result.FromUtc);
        Assert.NotNull(result.ToUtc);
        Assert.Equal(toUtc, result.ToUtc);
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(3, paginationData.CurrentPage);
        Assert.Equal(25, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenTargetUserIdIsNull_ShouldMapNullTargetUserId()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create();

        // Act
        GetMostConsumedQuery result = request.ToQuery(null);

        // Assert
        Assert.Null(result.TargetUserId);
    }

    [Fact]
    public void ToQuery_WhenCurrentPageAndPerPageAreNull_ShouldMapNullPaginationData()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            includeCurrentPage: false,
            includePerPage: false);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsSupplied_ShouldPreserveItAndDefaultPerPageToFifty()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            currentPage: 4,
            includePerPage: false);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(4, paginationData.CurrentPage);
        Assert.Equal(50, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsSupplied_ShouldPreserveItAndDefaultCurrentPageToOne()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            includeCurrentPage: false,
            perPage: 20);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(1, paginationData.CurrentPage);
        Assert.Equal(20, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenCurrentPageAndPerPageAreSupplied_ShouldPreserveBoth()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            currentPage: 7,
            perPage: 15);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(7, paginationData.CurrentPage);
        Assert.Equal(15, paginationData.PerPage);
    }

    [Theory]
    [InlineData(0, 0, 1, 1)] // both at the lower boundary
    [InlineData(0, -5, 1, 1)] // zero current page with a negative page size
    [InlineData(-3, 10, 1, 10)] // negative current page with a valid page size
    [InlineData(-2, -7, 1, 1)] // both negative
    public void ToQuery_WhenCurrentPageOrPerPageIsNotPositive_ShouldClampOnlyTheInvalidValue(
        int currentPage,
        int perPage,
        int expectedCurrentPage,
        int expectedPerPage)
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            currentPage: currentPage,
            perPage: perPage);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(expectedCurrentPage, paginationData.CurrentPage);
        Assert.Equal(expectedPerPage, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(includeLibraryId: false);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(includeMediaKind: false);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.MediaKind);
    }

    [Fact]
    public void ToQuery_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(includeFromUtc: false);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.FromUtc);
        Assert.NotNull(result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(includeToUtc: false);

        // Act
        GetMostConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ToUtc);
        Assert.NotNull(result.FromUtc);
    }
}