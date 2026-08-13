#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly IQueryHandler<GetFilesQuery, ErrorOr<IEnumerable<FileResponse>>> _getFilesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpoint"/> class.
    /// </summary>
    /// <param name="getFilesQueryHandler">Injected service for handling get files queries.</param>
    public GetFilesEndpoint(IQueryHandler<GetFilesQuery, ErrorOr<IEnumerable<FileResponse>>> getFilesQueryHandler)
    {
        _getFilesQueryHandler = getFilesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Files.GET_FILES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the files of the path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the files.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetFilesRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileResponse>> result = await _getFilesQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
