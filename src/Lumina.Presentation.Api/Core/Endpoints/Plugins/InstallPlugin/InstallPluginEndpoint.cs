#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Commands.InstallPlugin;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// API endpoint for the <c>/plugins</c> route.
/// </summary>
public class InstallPluginEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<InstallPluginCommand, Result<Success>> _installPluginCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpoint"/> class.
    /// </summary>
    /// <param name="installPluginCommandHandler">Injected service for handling install plugin commands.</param>
    public InstallPluginEndpoint(ICommandHandler<InstallPluginCommand, Result<Success>> installPluginCommandHandler)
    {
        _installPluginCommandHandler = installPluginCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Plugins.INSTALL_PLUGIN);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Installs the plugin uploaded in the multipart form of the request, placing its assemblies into the plugin storage directory.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // a multipart body without any part cannot be parsed as a form, so a malformed upload is treated as a missing archive
        IFormFile? archive = null;
        if (HttpContext.Request.HasFormContentType)
        {
            try
            {
                archive = HttpContext.Request.Form.Files.FirstOrDefault();
            }
            catch (InvalidDataException)
            {
                archive = null;
            }
        }

        InstallPluginCommand command = new(archive?.OpenReadStream(), archive?.FileName);
        Result<Success> result = await _installPluginCommandHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(), Problem);
    }
}
