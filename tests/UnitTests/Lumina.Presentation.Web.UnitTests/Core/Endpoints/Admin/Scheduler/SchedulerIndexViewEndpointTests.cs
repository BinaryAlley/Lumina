#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler;

/// <summary>
/// Contains unit tests for the <see cref="SchedulerIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SchedulerIndexViewEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IOptionsSnapshot<ServerConfigurationDto> _mockServerConfigurationOptions;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly SchedulerIndexViewEndpoint _sut;
    private readonly ServerConfigurationDtoFixture _serverConfigurationDtoFixture = new();
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerIndexViewEndpointTests"/> class.
    /// </summary>
    public SchedulerIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockAntiforgery = Substitute.For<IAntiforgery>();
        _mockAntiforgery.GetAndStoreTokens(Arg.Any<HttpContext>())
            .Returns(new AntiforgeryTokenSet("test-request-token", "test-cookie-token", "__RequestVerificationToken", "RequestVerificationToken"));
        _mockServerConfigurationOptions = Substitute.For<IOptionsSnapshot<ServerConfigurationDto>>();
        _mockServerConfigurationOptions.Value.Returns(_serverConfigurationDtoFixture.Create(baseAddress: "http://localhost", port: 5214));
        _mockUrlService = Substitute.For<IUrlService>();
        _mockThemePageRenderer = Substitute.For<ThemePageRenderer>(new ThemeService(_mockApiHttpClient), new ThemeTemplateEngine());
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
        _mockStringLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(
        [
            new LocalizedString("ScheduledJobs", "Scheduled Jobs"),
            new LocalizedString("TaskType.ScanMediaLibraries", "Scan media libraries"),
            new LocalizedString("TaskType.CleanTemporaryFiles", "Clean temporary files"),
            new LocalizedString("TaskType.RepairThemes", "Repair themes"),
            new LocalizedString("TaskType.CleanScheduledJobExecutionHistory", "Clean execution history"),
            new LocalizedString("Status.Added", "Added"),
            new LocalizedString("Status.Active", "Active"),
            new LocalizedString("Status.Running", "Running"),
            new LocalizedString("Status.Completed", "Completed")
        ]);
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<SchedulerIndexViewEndpoint>(_mockAntiforgery, _mockServerConfigurationOptions, _mockUrlService, _mockThemePageRenderer, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/admin/scheduled-jobs";
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldRenderThemedPageThroughTheSharedView()
    {
        // Arrange
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Scheduler.GET_SCHEDULED_JOBS).Returns("http://localhost/en-us/admin/api-scheduled-jobs");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create(content: "<section>content</section>", script: "<script>script</script>")));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "admin/scheduler" &&
                page.Title == "ScheduledJobs" &&
                string.Equals(page.PageData["scheduledJobsHubUrl"] as string, "http://localhost:5214/scheduledJobsHub", StringComparison.Ordinal) &&
                string.Equals(page.PageData["jobsListUrl"] as string, "http://localhost/en-us/admin/api-scheduled-jobs", StringComparison.Ordinal) &&
                string.Equals(page.PageData["antiforgeryToken"] as string, "test-request-token", StringComparison.Ordinal) &&
                page.PageData.ContainsKey("strings")),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToRazorView()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockThemePageRenderer.Received(1).RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldStoreAntiforgeryTokenAndRequestSchedulerUrls()
    {
        // Arrange
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Scheduler.GET_DISPLAY_PREFERENCES).Returns("http://localhost/en-us/admin/api-scheduled-jobs/display-preferences");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Scheduler.START_SCHEDULED_JOB, Arg.Any<object>()).Returns("http://localhost/en-us/admin/api-scheduled-jobs/00000000-0000-0000-0000-000000000000/start");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        _mockAntiforgery.Received(1).GetAndStoreTokens(_sut.HttpContext);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                string.Equals(page.PageData["getDisplayPreferencesUrl"] as string, "http://localhost/en-us/admin/api-scheduled-jobs/display-preferences", StringComparison.Ordinal) &&
                string.Equals(page.PageData["startJobUrlTemplate"] as string, "http://localhost/en-us/admin/api-scheduled-jobs/00000000-0000-0000-0000-000000000000/start", StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
