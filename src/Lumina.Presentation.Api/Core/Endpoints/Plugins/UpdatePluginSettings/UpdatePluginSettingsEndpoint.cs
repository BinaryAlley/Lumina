#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Presentation.Api.Common.Routes.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.UpdatePluginSettings;

/// <summary>
/// API endpoint for the <c>/plugins/{pluginId}/settings</c> route.
/// </summary>
public class UpdatePluginSettingsEndpoint : BaseEndpoint<UpdatePluginSettingsRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public UpdatePluginSettingsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.PUT);
        Routes(ApiRoutes.Plugins.UPDATE_PLUGIN_SETTINGS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the settings of the plugin identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the plugin and its settings.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdatePluginSettingsRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
