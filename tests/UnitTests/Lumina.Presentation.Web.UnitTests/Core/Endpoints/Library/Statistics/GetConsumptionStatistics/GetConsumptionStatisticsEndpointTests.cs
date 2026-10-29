#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;

/// <summary>
/// Contains unit tests for the <see cref="GetConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionStatisticsEndpointTests
{
    private static readonly DateTime s_fromUtc = new(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);
    private static readonly DateTime s_toUtc = new(2025, 6, 7, 8, 9, 10, DateTimeKind.Utc);

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetConsumptionStatisticsEndpoint _sut;
    private readonly GetConsumptionStatisticsRequestFixture _getConsumptionStatisticsRequestFixture = new();
    private readonly ConsumptionSummaryDtoFixture _consumptionSummaryDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    public GetConsumptionStatisticsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetConsumptionStatisticsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsStatistics_ShouldReturnSuccessJsonWithStatistics()
    {
        // Arrange
        ConsumptionSummaryDto expectedStatistics = _consumptionSummaryDtoFixture.Create();
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatistics);

        // Act
        IResult result = await _sut.ExecuteAsync(
            _getConsumptionStatisticsRequestFixture.Create(includeUserId: false, includeLibraryId: false, includeMediaKind: false, includeFromUtc: false, includeToUtc: false),
            CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        ConsumptionSummaryDto? returnedStatistics = jsonDocument.RootElement.GetProperty("data").Deserialize<ConsumptionSummaryDto>(_jsonOptions);
        Assert.NotNull(returnedStatistics);
        Assert.Equal(expectedStatistics.UserId, returnedStatistics.UserId);
        Assert.Equal(expectedStatistics.CountedSessionCount, returnedStatistics.CountedSessionCount);
    }

    [Theory]
    [InlineData(null)] // no user selected, the current user is implied
    [InlineData("")] // empty user selected
    [InlineData("   ")] // whitespace user selected
    public async Task ExecuteAsync_WhenUserIdIsNullOrWhitespace_ShouldRequestCurrentUserStatisticsWithoutQueryString(string? userId)
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            userId: userId,
            includeUserId: userId is not null,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_consumptionSummaryDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ConsumptionSummaryDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.LibraryStatistics.GET_CONSUMPTION_STATISTICS),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdAndAllFiltersAreSupplied_ShouldRequestUserStatisticsWithEscapedValues()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            userId: "user/../admin?x=1&y=2",
            libraryId: "lib?x=1&y=2",
            mediaKind: MediaKind.Movie,
            fromUtc: s_fromUtc,
            toUtc: s_toUtc);
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_consumptionSummaryDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ConsumptionSummaryDto>(
            Arg.Is<string>(endpoint => endpoint == "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/consumption-statistics?libraryId=lib%3Fx%3D1%26y%3D2&mediaKind=Movie&fromUtc=2024-03-04T05%3A06%3A07.0000000Z&toUtc=2025-06-07T08%3A09%3A10.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyLibraryIdIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            libraryId: "lib-1",
            includeUserId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_consumptionSummaryDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ConsumptionSummaryDto>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics?libraryId=lib-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyMediaKindIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            mediaKind: MediaKind.Book,
            includeUserId: false,
            includeLibraryId: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_consumptionSummaryDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ConsumptionSummaryDto>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics?mediaKind=Book"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyFromUtcIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            fromUtc: s_fromUtc,
            includeUserId: false,
            includeLibraryId: false,
            includeMediaKind: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_consumptionSummaryDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ConsumptionSummaryDto>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics?fromUtc=2024-03-04T05%3A06%3A07.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyToUtcIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            toUtc: s_toUtc,
            includeUserId: false,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false);
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_consumptionSummaryDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ConsumptionSummaryDto>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics?toUtc=2025-06-07T08%3A09%3A10.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        _mockApiHttpClient.GetAsync<ConsumptionSummaryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_consumptionSummaryDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }
}
