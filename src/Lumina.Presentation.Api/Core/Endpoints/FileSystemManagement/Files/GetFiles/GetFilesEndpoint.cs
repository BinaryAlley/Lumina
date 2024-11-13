#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Files.GetFiles;

/// <summary>
/// API endpoint for the <c>/files/get-files</c> route.
/// </summary>
public class GetFilesEndpoint : BaseEndpoint<GetFilesRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetFilesEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Files.GET_FILES);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the files of the path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the files.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetFilesRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileResponse>> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
