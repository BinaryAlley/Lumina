#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Core.Endpoints.Common;

/// <summary>
/// Fixture class for the <see cref="BaseEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
[DontRegister]
public class BaseEndpointFixture : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Exposes the <c>Culture</c> property of the <see cref="BaseEndpoint"/> class.
    /// </summary>
    public string TestCulture => Culture;

    /// <summary>
    /// Renders a Razor view through the <see cref="BaseEndpoint.View(string, object?, IReadOnlyDictionary{string, object?})"/> helper.
    /// </summary>
    /// <param name="viewName">The name or path of the view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="viewData">Optional additional view data entries made available to the view via <c>ViewData</c>.</param>
    /// <returns>An <see cref="IResult"/> that renders the view.</returns>
    public IResult TestView(string viewName, object? model = null, IReadOnlyDictionary<string, object?>? viewData = null)
    {
        return View(viewName, model, viewData);
    }

    /// <summary>
    /// Creates a JSON response indicating a successful operation through the <see cref="BaseEndpoint.JsonSuccess(object?)"/> helper.
    /// </summary>
    /// <param name="data">The payload of the successful response.</param>
    /// <returns>An <see cref="IResult"/> containing the success JSON payload.</returns>
    public IResult TestJsonSuccess(object? data)
    {
        return JsonSuccess(data);
    }

    /// <summary>
    /// Creates a JSON response indicating a successful operation without a payload through the <see cref="BaseEndpoint.JsonSuccess()"/> helper.
    /// </summary>
    /// <returns>An <see cref="IResult"/> containing the success JSON payload.</returns>
    public IResult TestJsonSuccess()
    {
        return JsonSuccess();
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
    }

    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="IResult"/> containing the success JSON payload.</returns>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(TestJsonSuccess());
    }
}
