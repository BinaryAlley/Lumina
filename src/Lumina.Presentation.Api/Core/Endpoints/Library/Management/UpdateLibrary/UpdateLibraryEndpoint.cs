#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.UpdateLibrary;

/// <summary>
/// API endpoint for the <c>/libraries/{id}</c> route.
/// </summary>
public class UpdateLibraryEndpoint : BaseEndpoint<UpdateLibraryRequest, IResult>
{
    private readonly ICommandHandler<UpdateLibraryCommand, ErrorOr<LibraryResponse>> _updateLibraryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="updateLibraryCommandHandler">Injected service for handling update library commands.</param>
    public UpdateLibraryEndpoint(ICommandHandler<UpdateLibraryCommand, ErrorOr<LibraryResponse>> updateLibraryCommandHandler)
    {
        _updateLibraryCommandHandler = updateLibraryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Libraries.UPDATE_LIBRARY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates a media library stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the media library to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateLibraryRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<LibraryResponse> result = await _updateLibraryCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
