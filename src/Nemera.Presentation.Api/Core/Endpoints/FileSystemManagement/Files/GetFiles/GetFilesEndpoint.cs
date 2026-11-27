#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.FileSystemManagement.Files;
using Nemera.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using Nemera.Contracts.Requests.FileSystemManagement.Files;
using Nemera.Contracts.Responses.FileSystemManagement.Files;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.FileSystemManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.FileSystemManagement.Files.GetFiles;

/// <summary>
/// API endpoint for the <c>/files/get-files</c> route.
/// </summary>
public class GetFilesEndpoint : BaseEndpoint<GetFilesRequest, IResult>
{
    private readonly IQueryHandler<GetFilesQuery, Result<IEnumerable<FileResponse>>> _getFilesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpoint"/> class.
    /// </summary>
    /// <param name="getFilesQueryHandler">Injected service for handling get files queries.</param>
    public GetFilesEndpoint(IQueryHandler<GetFilesQuery, Result<IEnumerable<FileResponse>>> getFilesQueryHandler)
    {
        _getFilesQueryHandler = getFilesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
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
        Result<IEnumerable<FileResponse>> result = await _getFilesQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
