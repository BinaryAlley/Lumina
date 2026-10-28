#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetGenreConsumption;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.GetGenreConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetGenreConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionEndpointTests
{
    private static readonly DateTime s_fromUtc = new(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);
    private static readonly DateTime s_toUtc = new(2025, 6, 7, 8, 9, 10, DateTimeKind.Utc);

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetGenreConsumptionEndpoint _sut;
    private readonly GetGenreConsumptionRequestFixture _getGenreConsumptionRequestFixture = new();
    private readonly GenreConsumptionDtoFixture _genreConsumptionDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionEndpointTests"/> class.
    /// </summary>
    public GetGenreConsumptionEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetGenreConsumptionEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsGenres_ShouldReturnSuccessJsonWithGenres()
    {
        // Arrange
        GenreConsumptionDto[] expectedGenres = [.. _genreConsumptionDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedGenres);

        // Act
        IResult result = await _sut.ExecuteAsync(
            _getGenreConsumptionRequestFixture.Create(includeUserId: false, includeLibraryId: false, includeMediaKind: false, includeFromUtc: false, includeToUtc: false),
            CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        GenreConsumptionDto[]? returnedGenres = jsonDocument.RootElement.GetProperty("data").Deserialize<GenreConsumptionDto[]>(_jsonOptions);
        Assert.NotNull(returnedGenres);
        Assert.Equal(expectedGenres.Select(genre => genre.Name), returnedGenres.Select(genre => genre.Name));
        Assert.Equal(expectedGenres.Select(genre => genre.ConsumedItemCount), returnedGenres.Select(genre => genre.ConsumedItemCount));
    }

    [Theory]
    [InlineData(null)] // no user selected, the current user is implied
    [InlineData("")] // empty user selected
    [InlineData("   ")] // whitespace user selected
    public async Task ExecuteAsync_WhenUserIdIsNullOrWhitespace_ShouldRequestCurrentUserGenresWithoutQueryString(string? userId)
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            userId: userId,
            includeUserId: userId is not null,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _genreConsumptionDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<GenreConsumptionDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.LibraryStatistics.GET_GENRE_CONSUMPTION),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdAndAllFiltersAreSupplied_ShouldRequestUserGenresWithEscapedValues()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            userId: "user/../admin?x=1&y=2",
            libraryId: "lib?x=1&y=2",
            mediaKind: MediaKind.Movie,
            fromUtc: s_fromUtc,
            toUtc: s_toUtc);
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _genreConsumptionDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<GenreConsumptionDto[]>(
            Arg.Is<string>(endpoint => endpoint == "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/consumption-statistics/genres?libraryId=lib%3Fx%3D1%26y%3D2&mediaKind=Movie&fromUtc=2024-03-04T05%3A06%3A07.0000000Z&toUtc=2025-06-07T08%3A09%3A10.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyLibraryIdIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            libraryId: "lib-1",
            includeUserId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _genreConsumptionDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<GenreConsumptionDto[]>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics/genres?libraryId=lib-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyMediaKindIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            mediaKind: MediaKind.Book,
            includeUserId: false,
            includeLibraryId: false,
            includeFromUtc: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _genreConsumptionDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<GenreConsumptionDto[]>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics/genres?mediaKind=Book"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyFromUtcIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            fromUtc: s_fromUtc,
            includeUserId: false,
            includeLibraryId: false,
            includeMediaKind: false,
            includeToUtc: false);
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _genreConsumptionDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<GenreConsumptionDto[]>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics/genres?fromUtc=2024-03-04T05%3A06%3A07.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyToUtcIsSupplied_ShouldOmitEveryOtherFilter()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            toUtc: s_toUtc,
            includeUserId: false,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false);
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _genreConsumptionDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<GenreConsumptionDto[]>(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-statistics/genres?toUtc=2025-06-07T08%3A09%3A10.0000000Z"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create();
        _mockApiHttpClient.GetAsync<GenreConsumptionDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _genreConsumptionDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }
}
