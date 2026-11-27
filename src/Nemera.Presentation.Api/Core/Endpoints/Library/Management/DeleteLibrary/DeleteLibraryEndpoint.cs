#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;
using Nemera.Contracts.Requests.MediaLibrary.Management;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.Management;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.Management.DeleteLibrary;

/// <summary>
/// API endpoint for the <c>/libraries/{id}</c> route.
/// </summary>
public class DeleteLibraryEndpoint : BaseEndpoint<DeleteLibraryRequest, IResult>
{
    private readonly ICommandHandler<DeleteLibraryCommand, Result<Deleted>> _deleteLibraryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="deleteLibraryCommandHandler">Injected service for handling delete library commands.</param>
    public DeleteLibraryEndpoint(ICommandHandler<DeleteLibraryCommand, Result<Deleted>> deleteLibraryCommandHandler)
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
        Result<Deleted> result = await _deleteLibraryCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
