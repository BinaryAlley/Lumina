#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Queries.GetPlugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// API endpoint for the <c>/plugins</c> route.
/// </summary>
public class GetPluginsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetPluginsQuery, Result<IReadOnlyList<PluginResponse>>> _getPluginsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpoint"/> class.
    /// </summary>
    /// <param name="getPluginsQueryHandler">Injected service for handling get plugins queries.</param>
    public GetPluginsEndpoint(IQueryHandler<GetPluginsQuery, Result<IReadOnlyList<PluginResponse>>> getPluginsQueryHandler)
    {
        _getPluginsQueryHandler = getPluginsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Plugins.GET_PLUGINS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of all the detected plugins.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<PluginResponse>> result = await _getPluginsQueryHandler.HandleAsync(new GetPluginsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
