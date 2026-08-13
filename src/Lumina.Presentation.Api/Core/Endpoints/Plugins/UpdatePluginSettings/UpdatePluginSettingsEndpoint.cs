#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Presentation.Api.Common.Routes.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly ICommandHandler<UpdatePluginSettingsCommand, ErrorOr<Success>> _updatePluginSettingsCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="updatePluginSettingsCommandHandler">Injected service for handling update plugin settings commands.</param>
    public UpdatePluginSettingsEndpoint(ICommandHandler<UpdatePluginSettingsCommand, ErrorOr<Success>> updatePluginSettingsCommandHandler)
    {
        _updatePluginSettingsCommandHandler = updatePluginSettingsCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
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
        ErrorOr<Success> result = await _updatePluginSettingsCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
