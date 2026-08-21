#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Renders a Razor partial to a string, so that the application chrome can be reused by the themed layout renderer.
/// </summary>
public sealed class RazorViewToStringRenderer
{
    private readonly ICompositeViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorViewToStringRenderer"/> class.
    /// </summary>
    /// <param name="viewEngine">Injected engine used to resolve the partial to render.</param>
    /// <param name="tempDataProvider">Injected provider used to create the temp data dictionary of the view context.</param>
    /// <param name="serviceProvider">Injected service provider used to resolve the model metadata provider.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    public RazorViewToStringRenderer(ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Renders the partial with the specified name to a string.
    /// </summary>
    /// <param name="partialName">The name of the partial to render.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The rendered HTML of the partial.</returns>
    public async Task<string> RenderPartialAsync(string partialName, CancellationToken cancellationToken = default)
    {
        // the theme shell reuses the application's Razor chrome as strings, so the same markup stays in one place
        HttpContext httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("The current HTTP context is unavailable.");
        ActionContext actionContext = new(httpContext, httpContext.GetRouteData() ?? new RouteData(), new ActionDescriptor());
        IView view = _viewEngine.FindView(actionContext, partialName, isMainPage: false).View
            ?? throw new InvalidOperationException($"The partial '{partialName}' could not be found.");

        IModelMetadataProvider metadataProvider = _serviceProvider.GetRequiredService<IModelMetadataProvider>();
        ViewDataDictionary viewData = new(metadataProvider, new ModelStateDictionary());
        using StringWriter writer = new();
        ViewContext viewContext = new(actionContext, view, viewData, new TempDataDictionary(httpContext, _tempDataProvider), writer, new HtmlHelperOptions());
        await view.RenderAsync(viewContext).ConfigureAwait(false);
        return writer.ToString();
    }
}
