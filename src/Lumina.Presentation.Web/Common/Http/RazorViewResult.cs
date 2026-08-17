#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Http;

/// <summary>
/// <see cref="IResult"/> that renders a Razor view through the MVC view engine, enabling FastEndpoints endpoints to serve the Razor views of the application.
/// </summary>
/// <remarks>
/// The <see cref="Results"/> class does not expose a method for rendering Razor views, so this custom result reuses the MVC
/// <see cref="ViewResult"/> execution machinery, which preserves layouts, sections, view components and tag helpers.
/// </remarks>
public sealed class RazorViewResult : IResult
{
    private readonly string _viewName;
    private readonly object? _model;
    private readonly IReadOnlyDictionary<string, object?> _viewData;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorViewResult"/> class.
    /// </summary>
    /// <param name="viewName">The name or path of the view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="viewData">Optional additional view data entries made available to the view via <c>ViewData</c>.</param>
    public RazorViewResult(string viewName, object? model = null, IReadOnlyDictionary<string, object?>? viewData = null)
    {
        _viewName = viewName;
        _model = model;
        _viewData = viewData ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Renders the configured Razor view to the response.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        // the only accessible ViewDataDictionary constructors require either a model state or an existing dictionary, so start from an empty one and assign the model
        // through the Model setter; the setter rebases the model metadata on the runtime type of the value, which is what makes the asp-for tag helpers resolve the
        // properties of the actual model type instead of a plain object
        IModelMetadataProvider metadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
        ViewDataDictionary viewData = new(metadataProvider, new ModelStateDictionary()) { Model = _model };
        foreach ((string key, object? value) in _viewData)
            viewData[key] = value;

        // delegate to the MVC view execution machinery instead of rendering the view manually: it discovers the layout and _ViewStart, evaluates the sections,
        // and resolves the view components and tag helpers, none of which the Results class can do since it has no view rendering support
        ViewResult viewResult = new() { ViewName = _viewName, ViewData = viewData };
        IActionResultExecutor<ViewResult> executor = httpContext.RequestServices.GetRequiredService<IActionResultExecutor<ViewResult>>();
        // the view engine needs an ActionContext, which FastEndpoints does not provide; synthesize one that carries over the current route data, so that the
        // culture route value (and therefore the URL-based localization) keeps working while the view is rendered
        ActionContext actionContext = new(httpContext, httpContext.GetRouteData() ?? new RouteData(), new ActionDescriptor());
        await executor.ExecuteAsync(actionContext, viewResult).ConfigureAwait(false);
    }
}
