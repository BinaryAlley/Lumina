#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Core.Plugins.Queries.GetPluginSettings;
using Nemera.Contracts.Requests.Plugins;
using Nemera.Contracts.Responses.Plugins;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Plugins;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// API endpoint for the <c>/plugins/{pluginId}/settings</c> route.
/// </summary>
public class GetPluginSettingsEndpoint : BaseEndpoint<GetPluginSettingsRequest, IResult>
{
    private readonly IQueryHandler<GetPluginSettingsQuery, Result<PluginSettingsResponse>> _getPluginSettingsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="getPluginSettingsQueryHandler">Injected service for handling get plugin settings queries.</param>
    public GetPluginSettingsEndpoint(IQueryHandler<GetPluginSettingsQuery, Result<PluginSettingsResponse>> getPluginSettingsQueryHandler)
    {
        _getPluginSettingsQueryHandler = getPluginSettingsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Plugins.GET_PLUGIN_SETTINGS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the settings of the plugin identified by <paramref name="request"/> and their schema.
    /// </summary>
    /// <param name="request">The request containing the Id of the plugin whose settings are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPluginSettingsRequest request, CancellationToken cancellationToken)
    {
        Result<PluginSettingsResponse> result = await _getPluginSettingsQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
