#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// Contains unit tests for the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsEndpointTests
{
    private static readonly DateTime s_fromUtc = new(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);
    private static readonly DateTime s_toUtc = new(2025, 6, 7, 8, 9, 10, DateTimeKind.Utc);

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetAllUsersConsumptionStatisticsEndpoint _sut;
    private readonly GetAllUsersConsumptionStatisticsRequestFixture _getAllUsersConsumptionStatisticsRequestFixture = new();
    private readonly AllUsersConsumptionStatisticsDtoFixture _allUsersConsumptionStatisticsDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    public GetAllUsersConsumptionStatisticsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetAllUsersConsumptionStatisticsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsStatistics_ShouldReturnSuccessJsonWithStatistics()
    {
        // Arrange
        AllUsersConsumptionStatisticsDto expectedStatistics = _allUsersConsumptionStatisticsDtoFixture.Create();
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatistics);

        // Act
        IResult result = await _sut.ExecuteAsync(
            _getAllUsersConsumptionStatisticsRequestFixture.Create(includeLibraryId: false, includeMediaKind: false, includeFromUtc: false, includeToUtc: false),
            CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        AllUsersConsumptionStatisticsDto? returnedStatistics = jsonDocument.RootElement.GetProperty("data").Deserialize<AllUsersConsumptionStatisticsDto>(_jsonOptions);
        Assert.NotNull(returnedStatistics);
        Assert.Equal(expectedStatistics.Total.CountedSessionCount, returnedStatistics.Total.CountedSessionCount);
        Assert.Equal(
            expectedStatistics.Users.Select(userStatistics => userStatistics.UserId),
            returnedStatistics.Users.Select(userStatistics => userStatistics.UserId));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFilterIsSupplied_ShouldRequestAllUsersStatisticsWithoutQueryString()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<AllUsersConsumptionStatisticsDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.LibraryStatistics.GET_ALL_USERS_CONSUMPTION_STATISTICS),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllFiltersAreSupplied_ShouldAppendEveryFilterToTheUpstreamQuery()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: "lib-1",
            mediaKind: MediaKind.Movie,
            fromUtc: s_fromUtc,
            toUtc: s_toUtc);
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<AllUsersConsumptionStatisticsDto>(
            Arg.Is<string>(endpoint => endpoint == "users/consumption-statistics?libraryId=lib-1&mediaKind=Movie&fromUtc=2024-03-04T05%3A06%3A07.0000000Z&toUtc=2025-06-07T08%3A09%3A10.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyLibraryIdIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: "lib-1",
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<AllUsersConsumptionStatisticsDto>(
            Arg.Is<string>(endpoint => endpoint == "users/consumption-statistics?libraryId=lib-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyMediaKindIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            mediaKind: MediaKind.Book,
            includeLibraryId: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<AllUsersConsumptionStatisticsDto>(
            Arg.Is<string>(endpoint => endpoint == "users/consumption-statistics?mediaKind=Book"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyFromUtcIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            fromUtc: s_fromUtc,
            includeLibraryId: false,
            includeMediaKind: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<AllUsersConsumptionStatisticsDto>(
            Arg.Is<string>(endpoint => endpoint == "users/consumption-statistics?fromUtc=2024-03-04T05%3A06%3A07.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyToUtcIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            toUtc: s_toUtc,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false);
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<AllUsersConsumptionStatisticsDto>(
            Arg.Is<string>(endpoint => endpoint == "users/consumption-statistics?toUtc=2025-06-07T08%3A09%3A10.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryIdContainsUrlSyntax_ShouldEscapeItInTheUpstreamQuery()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: "lib?x=1&y=2",
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<AllUsersConsumptionStatisticsDto>(
            Arg.Is<string>(endpoint => endpoint == "users/consumption-statistics?libraryId=lib%3Fx%3D1%26y%3D2"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        _mockApiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_allUsersConsumptionStatisticsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public void Configure_WhenCalled_ShouldRestrictTheRouteToAdministrators()
    {
        // Act
        _sut.Configure();

        // Assert
        Assert.NotNull(_sut.Definition.PreBuiltUserPolicies);
        Assert.Contains(AuthorizationPolicies.REQUIRE_ADMIN_ROLE, _sut.Definition.PreBuiltUserPolicies);
        Assert.Contains("GET", _sut.Definition.Verbs);
        Assert.Contains(WebRoutes.LibraryStatistics.GET_ALL_USERS_STATISTICS, _sut.Definition.Routes);
    }
}
