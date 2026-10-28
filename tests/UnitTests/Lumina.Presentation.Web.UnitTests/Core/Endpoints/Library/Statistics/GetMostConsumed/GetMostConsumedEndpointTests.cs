#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetMostConsumed;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.GetMostConsumed;

/// <summary>
/// Contains unit tests for the <see cref="GetMostConsumedEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedEndpointTests
{
    private static readonly DateTime s_fromUtc = new(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);
    private static readonly DateTime s_toUtc = new(2025, 6, 7, 8, 9, 10, DateTimeKind.Utc);

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetMostConsumedEndpoint _sut;
    private readonly GetMostConsumedRequestFixture _getMostConsumedRequestFixture = new();
    private readonly ConsumedItemDtoFixture _consumedItemDtoFixture = new();
    private readonly PaginatedDtoFixture<ConsumedItemDto> _paginatedDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedEndpointTests"/> class.
    /// </summary>
    public GetMostConsumedEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetMostConsumedEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsMostConsumedItems_ShouldReturnSuccessJsonWithPage()
    {
        // Arrange
        PaginatedDto<ConsumedItemDto> expectedPage = _paginatedDtoFixture.Create(
            data: [.. _consumedItemDtoFixture.CreateMany(2)],
            currentPage: 2,
            perPage: 5,
            count: 12,
            numberOfPages: 3);
        _mockApiHttpClient.GetAsync<PaginatedDto<ConsumedItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPage);

        // Act
        IResult result = await _sut.ExecuteAsync(
            _getMostConsumedRequestFixture.Create(includeUserId: false, includeLibraryId: false, includeMediaKind: false, includeFromUtc: false, includeToUtc: false),
            CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PaginatedDto<ConsumedItemDto>? returnedPage = jsonDocument.RootElement.GetProperty("data").Deserialize<PaginatedDto<ConsumedItemDto>>(_jsonOptions);
        Assert.NotNull(returnedPage);
        Assert.Equal(expectedPage.Count, returnedPage.Count);
        Assert.Equal(expectedPage.CurrentPage, returnedPage.CurrentPage);
        Assert.Equal(expectedPage.PerPage, returnedPage.PerPage);
        Assert.Equal(expectedPage.NumberOfPages, returnedPage.NumberOfPages);
        Assert.Equal(expectedPage.Data.Select(item => item.MediaItemId), returnedPage.Data.Select(item => item.MediaItemId));
    }

    [Theory]
    [InlineData(null)] // no user selected, the current user is implied
    [InlineData("")] // empty user selected
    [InlineData("   ")] // whitespace user selected
    public async Task ExecuteAsync_WhenUserIdIsNullOrWhitespaceAndNoFilterIsSupplied_ShouldRequestCurrentUserItemsWithDefaultPaging(string? userId)
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            userId: userId,
            includeUserId: userId is not null,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false,
            includeCurrentPage: false,
            includePerPage: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<ConsumedItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create(data: []));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<ConsumedItemDto>>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-history/most-consumed?currentPage=1&perPage=10"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyPagingIsSupplied_ShouldOmitEveryOptionalFilter()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            currentPage: 3,
            perPage: 25,
            includeUserId: false,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<ConsumedItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create(data: []));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<ConsumedItemDto>>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-history/most-consumed?currentPage=3&perPage=25"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyLibraryIdIsSupplied_ShouldOmitEveryOtherFilterAndUseDefaultPaging()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            libraryId: "lib-1",
            includeUserId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false,
            includeCurrentPage: false,
            includePerPage: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<ConsumedItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create(data: []));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<ConsumedItemDto>>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-history/most-consumed?libraryId=lib-1&currentPage=1&perPage=10"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdAndAllFiltersAreSupplied_ShouldRequestUserItemsWithEscapedValues()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            userId: "user/../admin?x=1&y=2",
            libraryId: "lib?x=1&y=2",
            mediaKind: MediaKind.Movie,
            fromUtc: s_fromUtc,
            toUtc: s_toUtc,
            currentPage: 4,
            perPage: 20);
        _mockApiHttpClient.GetAsync<PaginatedDto<ConsumedItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create(data: []));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<ConsumedItemDto>>(
            Arg.Is<string>(endpoint => endpoint == "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/consumption-history/most-consumed?libraryId=lib%3Fx%3D1%26y%3D2&mediaKind=Movie&fromUtc=2024-03-04T05%3A06%3A07.0000000Z&toUtc=2025-06-07T08%3A09%3A10.0000000Z&currentPage=4&perPage=20"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create();
        _mockApiHttpClient.GetAsync<PaginatedDto<ConsumedItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create(data: []));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }
}
