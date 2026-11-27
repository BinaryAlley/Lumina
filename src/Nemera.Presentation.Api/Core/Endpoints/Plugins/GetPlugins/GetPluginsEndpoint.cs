#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.Plugins.Queries.GetPlugins;
using Nemera.Contracts.Responses.Plugins;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Plugins;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Plugins.GetPlugins;

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
    /// <param name="_">The empty request, which carries no data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<PluginResponse>> result = await _getPluginsQueryHandler.HandleAsync(new GetPluginsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
