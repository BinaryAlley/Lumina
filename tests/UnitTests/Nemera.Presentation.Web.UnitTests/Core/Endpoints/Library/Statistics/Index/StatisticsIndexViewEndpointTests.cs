#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Authorization;
using Nemera.Presentation.Web.Common.DTO.Themes;
using Nemera.Presentation.Web.Common.Http;
using Nemera.Presentation.Web.Common.Primitives;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Common.Services;
using Nemera.Presentation.Web.Core.Endpoints.Library.Statistics.Index;
using Nemera.Presentation.Web.Core.Themes;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Themes;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.Index;

/// <summary>
/// Contains unit tests for the <see cref="StatisticsIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatisticsIndexViewEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly StatisticsIndexViewEndpoint _sut;
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsIndexViewEndpointTests"/> class.
    /// </summary>
    public StatisticsIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockUrlService = Substitute.For<IUrlService>();
        _mockThemePageRenderer = Substitute.For<ThemePageRenderer>(new ThemeService(_mockApiHttpClient), new ThemeTemplateEngine());
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
        _mockStringLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), $"localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(
        [
            new LocalizedString("Title", "localized-Title"),
            new LocalizedString("Loading", "localized-Loading")
        ]);
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _sut = Factory.Create<StatisticsIndexViewEndpoint>(_mockUrlService, _mockThemePageRenderer, _mockStringLocalizerFactory, _mockAuthorizationService);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        TestHttpContextFactory.ConfigureCulture(_sut.HttpContext, "en-US");
        _sut.HttpContext.Request.Path = "/en-us/library/statistics";
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldReturnThemedRazorViewResult()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create(content: "<section>content</section>", script: "<script>script</script>")));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Any<ThemePageDto>(),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldPassLocalizedTitleAndPageDataToTheThemeRenderer()
    {
        // Arrange
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_STATISTICS).Returns("http://localhost/en-us/library/statistics/api-get-statistics");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "library/statistics/index" &&
                page.Title == "localized-Title" &&
                page.PageData.ContainsKey("getStatisticsUrl") &&
                page.PageData.ContainsKey("getAllUsersStatisticsUrl") &&
                page.PageData.ContainsKey("getRecentlyConsumedUrl") &&
                page.PageData.ContainsKey("getMostConsumedUrl") &&
                page.PageData.ContainsKey("getTagConsumptionUrl") &&
                page.PageData.ContainsKey("getGenreConsumptionUrl") &&
                page.PageData.ContainsKey("getLibrariesUrl") &&
                page.PageData.ContainsKey("getUsersUrl") &&
                page.PageData.ContainsKey("deleteConsumptionItemHistoryUrl") &&
                page.PageData.ContainsKey("clearConsumptionHistoryUrl") &&
                page.PageData.ContainsKey("clearResumePointsUrl") &&
                page.PageData.ContainsKey("culture") &&
                page.PageData.ContainsKey("isAdmin") &&
                page.PageData.ContainsKey("currentUserId") &&
                page.PageData.ContainsKey("strings") &&
                string.Equals(
                    page.PageData["getStatisticsUrl"] as string,
                    "http://localhost/en-us/library/statistics/api-get-statistics",
                    StringComparison.Ordinal)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldReturnRazorViewResult()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Any<ThemePageDto>(),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsAdmin_ShouldExposeAdminFlagUsersUrlAndCurrentUserId()
    {
        // Arrange
        Guid expectedUserId = Guid.NewGuid();
        ConfigureCurrentUser(expectedUserId.ToString());
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_USERS).Returns("http://localhost/en-us/library/statistics/api-get-users");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                Equals(page.PageData["isAdmin"], true) &&
                string.Equals(
                    page.PageData["getUsersUrl"] as string,
                    "http://localhost/en-us/library/statistics/api-get-users",
                    StringComparison.Ordinal) &&
                page.PageData["currentUserId"] != null &&
                (Guid)page.PageData["currentUserId"]! == expectedUserId),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsNotAdmin_ShouldHideUsersUrl()
    {
        // Arrange
        ConfigureCurrentUser(Guid.NewGuid().ToString());
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(false);
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_USERS).Returns("http://localhost/en-us/library/statistics/api-get-users");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                Equals(page.PageData["isAdmin"], false) &&
                string.Equals(page.PageData["getUsersUrl"] as string, string.Empty, StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        _mockUrlService.DidNotReceive().GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_USERS);
    }

    [Theory]
    [InlineData(null)] // No identifier claim at all.
    [InlineData("")] // Empty identifier claim.
    [InlineData("not-a-guid")] // Identifier claim that is not a GUID.
    public async Task ExecuteAsync_WhenNameIdentifierIsMissingOrNotAGuid_ShouldLeaveCurrentUserIdNull(string? nameIdentifier)
    {
        // Arrange
        ConfigureCurrentUser(nameIdentifier);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page => page.PageData["currentUserId"] == null),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUrlServiceReturnsUrls_ShouldExposeEveryUrlFromTheRouteConstants()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_STATISTICS).Returns("https://nemera.test/statistics");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_ALL_USERS_STATISTICS).Returns("https://nemera.test/all-users-statistics");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_RECENTLY_CONSUMED).Returns("https://nemera.test/recently-consumed");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_MOST_CONSUMED).Returns("https://nemera.test/most-consumed");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_TAG_CONSUMPTION).Returns("https://nemera.test/tag-consumption");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_GENRE_CONSUMPTION).Returns("https://nemera.test/genre-consumption");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_LIBRARIES).Returns("https://nemera.test/libraries");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_USERS).Returns("https://nemera.test/users");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.DELETE_CONSUMPTION_ITEM_HISTORY).Returns("https://nemera.test/delete-consumption-item-history");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.CLEAR_CONSUMPTION_HISTORY).Returns("https://nemera.test/clear-consumption-history");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.CLEAR_RESUME_POINTS).Returns("https://nemera.test/clear-resume-points");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                string.Equals(page.PageData["getStatisticsUrl"] as string, "https://nemera.test/statistics", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getAllUsersStatisticsUrl"] as string, "https://nemera.test/all-users-statistics", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getRecentlyConsumedUrl"] as string, "https://nemera.test/recently-consumed", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getMostConsumedUrl"] as string, "https://nemera.test/most-consumed", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getTagConsumptionUrl"] as string, "https://nemera.test/tag-consumption", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getGenreConsumptionUrl"] as string, "https://nemera.test/genre-consumption", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getLibrariesUrl"] as string, "https://nemera.test/libraries", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getUsersUrl"] as string, "https://nemera.test/users", StringComparison.Ordinal) &&
                string.Equals(page.PageData["deleteConsumptionItemHistoryUrl"] as string, "https://nemera.test/delete-consumption-item-history", StringComparison.Ordinal) &&
                string.Equals(page.PageData["clearConsumptionHistoryUrl"] as string, "https://nemera.test/clear-consumption-history", StringComparison.Ordinal) &&
                string.Equals(page.PageData["clearResumePointsUrl"] as string, "https://nemera.test/clear-resume-points", StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_STATISTICS);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_ALL_USERS_STATISTICS);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_RECENTLY_CONSUMED);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_MOST_CONSUMED);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_TAG_CONSUMPTION);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_GENRE_CONSUMPTION);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_LIBRARIES);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_USERS);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.DELETE_CONSUMPTION_ITEM_HISTORY);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.CLEAR_CONSUMPTION_HISTORY);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.LibraryStatistics.CLEAR_RESUME_POINTS);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUrlServiceReturnsNull_ShouldExposeEmptyStrings()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                string.Equals(page.PageData["getStatisticsUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["getAllUsersStatisticsUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["getRecentlyConsumedUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["getMostConsumedUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["getTagConsumptionUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["getGenreConsumptionUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["getLibrariesUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["getUsersUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["deleteConsumptionItemHistoryUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["clearConsumptionHistoryUrl"] as string, string.Empty, StringComparison.Ordinal) &&
                string.Equals(page.PageData["clearResumePointsUrl"] as string, string.Empty, StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCultureRouteValueIsSet_ShouldExposeThatCulture()
    {
        // Arrange
        TestHttpContextFactory.ConfigureCulture(_sut.HttpContext, "fr-FR");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page => string.Equals(page.PageData["culture"] as string, "fr-FR", StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    /// <summary>
    /// Assigns a claims principal to the HTTP context of the endpoint under test.
    /// </summary>
    /// <param name="nameIdentifier">The value of the name identifier claim, or <see langword="null"/> to omit the claim.</param>
    private void ConfigureCurrentUser(string? nameIdentifier)
    {
        List<Claim> claims = nameIdentifier is null ? [] : [new Claim(ClaimTypes.NameIdentifier, nameIdentifier)];
        _sut.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));
    }
}
