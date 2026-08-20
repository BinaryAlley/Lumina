#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Home;

/// <summary>
/// API endpoint for the <c>/{culture}/not-found</c> route.
/// </summary>
public class HomeNotFoundViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // the resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Shared.NotFound";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // the page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope
    private const string VIEW_PAGE_KEY = "shared/not-found";

    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeNotFoundViewEndpoint"/> class.
    /// </summary>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public HomeNotFoundViewEndpoint(ThemePageRenderer themePageRenderer, IStringLocalizerFactory stringLocalizerFactory)
    {
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
        Routes(WebRoutes.Home.NOT_FOUND);
        DontAutoTag();
        Options(options => options.WithTags("Home"));
    }

    /// <summary>
    /// Displays the not-found page, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = _localizer["PageNotFound"].Value,
            Description = _localizer["ThereIsNoPage"].Value,
            PageData = new Dictionary<string, object?>
            {
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Shared/NotFound.cshtml");

        return View(
            "/Core/Views/Shared/_ThemedView.cshtml",
            new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script),
            new Dictionary<string, object?> { ["Title"] = pageModel.Title });
    }
}
