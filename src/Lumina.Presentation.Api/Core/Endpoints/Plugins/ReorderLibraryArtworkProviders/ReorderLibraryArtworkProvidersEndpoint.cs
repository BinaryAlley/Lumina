#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.ReorderLibraryArtworkProviders;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artwork-providers/reorder</c> route.
/// </summary>
public class ReorderLibraryArtworkProvidersEndpoint : BaseEndpoint<ReorderLibraryArtworkProvidersRequest, IResult>
{
    private readonly ICommandHandler<ReorderLibraryArtworkProvidersCommand, Result<Success>> _reorderLibraryArtworkProvidersCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryArtworkProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="reorderLibraryArtworkProvidersCommandHandler">Injected service for handling reorder library artwork providers commands.</param>
    public ReorderLibraryArtworkProvidersEndpoint(ICommandHandler<ReorderLibraryArtworkProvidersCommand, Result<Success>> reorderLibraryArtworkProvidersCommandHandler)
    {
        _reorderLibraryArtworkProvidersCommandHandler = reorderLibraryArtworkProvidersCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Libraries.REORDER_LIBRARY_ARTWORK_PROVIDERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Reorders the artwork providers of the media library identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library and the plugin Ids in the new order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ReorderLibraryArtworkProvidersRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _reorderLibraryArtworkProvidersCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
