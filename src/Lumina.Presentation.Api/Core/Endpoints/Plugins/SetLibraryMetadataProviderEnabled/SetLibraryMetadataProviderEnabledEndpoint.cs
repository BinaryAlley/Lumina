#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
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
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryMetadataProviderEnabledEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public SetLibraryMetadataProviderEnabledEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.PUT);
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
        ErrorOr<Success> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
