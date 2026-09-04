#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/scheduled-jobs</c> route.
/// </summary>
public class SchedulerIndexViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // the resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Admin.Scheduler";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // the page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope
    private const string VIEW_PAGE_KEY = "admin/scheduler";

    private readonly IAntiforgery _antiforgery;
    private readonly IOptionsSnapshot<ServerConfigurationDto> _serverConfigurationOptions;
    private readonly IUrlService _urlService;
    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="antiforgery">Injected service used to create the anti forgery tokens of the page.</param>
    /// <param name="serverConfigurationOptions">Injected options used to read the address of the API server that hosts the scheduled jobs hub.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public SchedulerIndexViewEndpoint(
        IAntiforgery antiforgery,
        IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions,
        IUrlService urlService,
        ThemePageRenderer themePageRenderer,
        IStringLocalizerFactory stringLocalizerFactory)
    {
        _antiforgery = antiforgery;
        _serverConfigurationOptions = serverConfigurationOptions;
        _urlService = urlService;
        _themePageRenderer = themePageRenderer;
        _localizer = stringLocalizerFactory.Create(VIEW_RESOURCE_BASE_NAME, VIEW_RESOURCE_LOCATION);
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Scheduler.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Displays the scheduled jobs dashboard, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        ServerConfigurationDto serverConfiguration = _serverConfigurationOptions.Value;
        Dictionary<string, object?> localizedStrings = ThemePageDataFactory.CreateLocalizedStrings(_localizer);

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = _localizer["ScheduledJobs"].Value,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["scheduledJobsHubUrl"] = $"{serverConfiguration.BaseAddress}:{serverConfiguration.Port}/scheduledJobsHub",
                ["jobsListUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.GET_SCHEDULED_JOBS) ?? string.Empty,
                ["addJobUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.ADD_SCHEDULED_JOB) ?? string.Empty,
                ["historyUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.GET_SCHEDULED_JOB_HISTORY) ?? string.Empty,
                ["startJobUrlTemplate"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.START_SCHEDULED_JOB, new { scheduledJobId = default(System.Guid) }) ?? string.Empty,
                ["stopJobUrlTemplate"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.STOP_SCHEDULED_JOB, new { scheduledJobId = default(System.Guid) }) ?? string.Empty,
                ["fireJobUrlTemplate"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.FIRE_SCHEDULED_JOB, new { scheduledJobId = default(System.Guid) }) ?? string.Empty,
                ["removeJobUrlTemplate"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.REMOVE_SCHEDULED_JOB, new { scheduledJobId = default(System.Guid) }) ?? string.Empty,
                ["accessTokenUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Authentication.GET_API_ACCESS_TOKEN) ?? string.Empty,
                ["getDisplayPreferencesUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.GET_DISPLAY_PREFERENCES) ?? string.Empty,
                ["updateDisplayPreferencesUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Scheduler.UPDATE_DISPLAY_PREFERENCES) ?? string.Empty,
                ["antiforgeryToken"] = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken ?? string.Empty,
                ["strings"] = localizedStrings,
                ["taskTypeNames"] = CreateSubgroup(localizedStrings, "TaskType", ["ScanMediaLibraries", "CleanTemporaryFiles", "RepairThemes", "CleanScheduledJobExecutionHistory"]),
                ["statusNames"] = CreateSubgroup(localizedStrings, "Status", ["Added", "Active", "Running", "Completed"])
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Admin/Scheduler.cshtml");

        return View(
            "/Core/Views/Shared/_ThemedView.cshtml",
            new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script),
            new Dictionary<string, object?> { ["Title"] = pageModel.Title });
    }

    /// <summary>
    /// Creates the subgroup of the localized strings whose resource names start with the provided prefix followed by a dot,
    /// keyed by the remainder of their names, so that a template can resolve them with non dotted expressions.
    /// </summary>
    /// <param name="localizedStrings">The localized strings keyed by their resource names.</param>
    /// <param name="prefix">The prefix of the resource names of the subgroup.</param>
    /// <param name="names">The names of the subgroup entries, without the prefix.</param>
    /// <returns>The localized strings of the subgroup keyed by their names.</returns>
    private static IReadOnlyDictionary<string, object?> CreateSubgroup(IReadOnlyDictionary<string, object?> localizedStrings, string prefix, IReadOnlyList<string> names)
    {
        Dictionary<string, object?> subgroup = new(StringComparer.OrdinalIgnoreCase);
        foreach (string name in names)
            if (localizedStrings.TryGetValue($"{prefix}.{name}", out object? value))
                subgroup[name] = value;

        return subgroup;
    }
}
