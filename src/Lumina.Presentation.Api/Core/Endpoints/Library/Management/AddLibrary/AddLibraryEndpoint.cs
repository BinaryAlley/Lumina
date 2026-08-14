#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.AddLibrary;

/// <summary>
/// API endpoint for the <c>/library</c> route.
/// </summary>
public class AddLibraryEndpoint : BaseEndpoint<AddLibraryRequest, IResult>
{
    private readonly ICommandHandler<AddLibraryCommand, Result<LibraryResponse>> _addLibraryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="addLibraryCommandHandler">Injected service for handling add library commands.</param>
    public AddLibraryEndpoint(ICommandHandler<AddLibraryCommand, Result<LibraryResponse>> addLibraryCommandHandler)
    {
        _addLibraryCommandHandler = addLibraryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Libraries.ADD_LIBRARY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds a media library stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the media library to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddLibraryRequest request, CancellationToken cancellationToken)
    {
        Result<LibraryResponse> result = await _addLibraryCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1{ApiRoutes.Libraries.ADD_LIBRARY}/{result.Value.Id}", result.Value), Problem);
    }
}
