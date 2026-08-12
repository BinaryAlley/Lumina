#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Presentation.Api.Common.Routes.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// API endpoint for the <c>/plugins/{pluginId}/settings</c> route.
/// </summary>
public class GetPluginSettingsEndpoint : BaseEndpoint<GetPluginSettingsRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetPluginSettingsEndpoint(ISender sender)
    {
        _sender = sender;
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
        ErrorOr<PluginSettingsResponse> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
