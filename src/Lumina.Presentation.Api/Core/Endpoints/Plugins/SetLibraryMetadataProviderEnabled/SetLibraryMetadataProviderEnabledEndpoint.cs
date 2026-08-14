#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.SetLibraryMetadataProviderEnabled;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/metadata-providers/{pluginId}/enabled</c> route.
/// </summary>
public class SetLibraryMetadataProviderEnabledEndpoint : BaseEndpoint<SetLibraryMetadataProviderEnabledRequest, IResult>
{
    private readonly ICommandHandler<SetLibraryMetadataProviderEnabledCommand, Result<Success>> _setLibraryMetadataProviderEnabledCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryMetadataProviderEnabledEndpoint"/> class.
    /// </summary>
    /// <param name="setLibraryMetadataProviderEnabledCommandHandler">Injected service for handling set library metadata provider enabled commands.</param>
    public SetLibraryMetadataProviderEnabledEndpoint(ICommandHandler<SetLibraryMetadataProviderEnabledCommand, Result<Success>> setLibraryMetadataProviderEnabledCommandHandler)
    {
        _setLibraryMetadataProviderEnabledCommandHandler = setLibraryMetadataProviderEnabledCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Libraries.SET_LIBRARY_METADATA_PROVIDER_ENABLED);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Enables or disables the metadata provider identified by <paramref name="request"/> for the media library.
    /// </summary>
    /// <param name="request">The request containing the Ids of the media library and of the metadata provider.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SetLibraryMetadataProviderEnabledRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _setLibraryMetadataProviderEnabledCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
