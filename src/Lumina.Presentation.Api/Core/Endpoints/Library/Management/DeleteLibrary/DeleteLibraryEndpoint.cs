#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.DeleteLibrary;

/// <summary>
/// API endpoint for the <c>//libraries/{id}</c> route.
/// </summary>
public class DeleteLibraryEndpoint : BaseEndpoint<DeleteLibraryRequest, IResult>
{
    private readonly ICommandHandler<DeleteLibraryCommand, ErrorOr<Deleted>> _deleteLibraryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="deleteLibraryCommandHandler">Injected service for handling delete library commands.</param>
    public DeleteLibraryEndpoint(ICommandHandler<DeleteLibraryCommand, ErrorOr<Deleted>> deleteLibraryCommandHandler)
    {
        _deleteLibraryCommandHandler = deleteLibraryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Libraries.DELETE_LIBRARY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes a library by Id.
    /// </summary>
    /// <param name="request">The request containing the id of the library to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteLibraryRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<Deleted> result = await _deleteLibraryCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
