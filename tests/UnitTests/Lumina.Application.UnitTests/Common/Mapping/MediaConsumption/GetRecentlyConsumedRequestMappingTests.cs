#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetRecentlyConsumed;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetRecentlyConsumedRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRecentlyConsumedRequestMappingTests
{
    private readonly GetRecentlyConsumedRequestFixture _getRecentlyConsumedRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRecentlyConsumedRequestMappingTests"/> class.
    /// </summary>
    public GetRecentlyConsumedRequestMappingTests()
    {
        _getRecentlyConsumedRequestFixture = new GetRecentlyConsumedRequestFixture();
    }

    [Fact]
    public void ToQuery_WhenMappingPopulatedRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2025-01-02T03:04:05Z";
        string toUtc = "2025-02-03T04:05:06Z";
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.TvEpisode),
            currentPage: 2,
            perPage: 40,
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(targetUserId.ToString());

        // Assert
        Assert.Equal(targetUserId.ToString(), result.TargetUserId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.NotNull(result.MediaKind);
        Assert.Equal(nameof(MediaKind.TvEpisode), result.MediaKind);
        Assert.NotNull(result.FromUtc);
        Assert.Equal(fromUtc, result.FromUtc);
        Assert.NotNull(result.ToUtc);
        Assert.Equal(toUtc, result.ToUtc);
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(2, paginationData.CurrentPage);
        Assert.Equal(40, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenTargetUserIdIsNull_ShouldMapNullTargetUserId()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create();

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(null);

        // Assert
        Assert.Null(result.TargetUserId);
    }

    [Fact]
    public void ToQuery_WhenCurrentPageAndPerPageAreNull_ShouldMapNullPaginationData()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(
            includeCurrentPage: false,
            includePerPage: false);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsSupplied_ShouldPreserveItAndDefaultPerPageToFifty()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(
            currentPage: 5,
            includePerPage: false);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(5, paginationData.CurrentPage);
        Assert.Equal(50, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsSupplied_ShouldPreserveItAndDefaultCurrentPageToOne()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(
            includeCurrentPage: false,
            perPage: 30);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(1, paginationData.CurrentPage);
        Assert.Equal(30, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenCurrentPageAndPerPageAreSupplied_ShouldPreserveBoth()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(
            currentPage: 9,
            perPage: 12);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(9, paginationData.CurrentPage);
        Assert.Equal(12, paginationData.PerPage);
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
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(
            currentPage: currentPage,
            perPage: perPage);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        PaginationDataDto paginationData = Assert.IsType<PaginationDataDto>(result.PaginationData);
        Assert.Equal(expectedCurrentPage, paginationData.CurrentPage);
        Assert.Equal(expectedPerPage, paginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(includeLibraryId: false);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(includeMediaKind: false);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.MediaKind);
    }

    [Fact]
    public void ToQuery_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(includeFromUtc: false);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.FromUtc);
        Assert.NotNull(result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetRecentlyConsumedRequest request = _getRecentlyConsumedRequestFixture.Create(includeToUtc: false);

        // Act
        GetRecentlyConsumedQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ToUtc);
        Assert.NotNull(result.FromUtc);
    }
}