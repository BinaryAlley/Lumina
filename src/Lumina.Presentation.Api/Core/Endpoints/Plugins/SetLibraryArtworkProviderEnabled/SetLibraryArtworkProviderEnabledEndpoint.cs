#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.SetLibraryArtworkProviderEnabled;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artwork-providers/{pluginId}/enabled</c> route.
/// </summary>
public class SetLibraryArtworkProviderEnabledEndpoint : BaseEndpoint<SetLibraryArtworkProviderEnabledRequest, IResult>
{
    private readonly ICommandHandler<SetLibraryArtworkProviderEnabledCommand, Result<Success>> _setLibraryArtworkProviderEnabledCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryArtworkProviderEnabledEndpoint"/> class.
    /// </summary>
    /// <param name="setLibraryArtworkProviderEnabledCommandHandler">Injected service for handling set library artwork provider enabled commands.</param>
    public SetLibraryArtworkProviderEnabledEndpoint(ICommandHandler<SetLibraryArtworkProviderEnabledCommand, Result<Success>> setLibraryArtworkProviderEnabledCommandHandler)
    {
        _setLibraryArtworkProviderEnabledCommandHandler = setLibraryArtworkProviderEnabledCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Libraries.SET_LIBRARY_ARTWORK_PROVIDER_ENABLED);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Enables or disables the artwork provider identified by <paramref name="request"/> for the media library.
    /// </summary>
    /// <param name="request">The request containing the Ids of the media library and of the artwork provider.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SetLibraryArtworkProviderEnabledRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _setLibraryArtworkProviderEnabledCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
