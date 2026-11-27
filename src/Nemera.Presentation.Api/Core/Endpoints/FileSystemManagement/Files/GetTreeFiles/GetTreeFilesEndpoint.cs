#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Files.GetTreeFiles;

/// <summary>
/// API endpoint for the <c>/files/get-tree-files</c> route.
/// </summary>
public class GetTreeFilesEndpoint : BaseEndpoint<GetTreeFilesRequest, IResult>
{
    private readonly IQueryHandler<GetTreeFilesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>> _getTreeFilesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesEndpoint"/> class.
    /// </summary>
    /// <param name="getTreeFilesQueryHandler">Injected service for handling get tree files queries.</param>
    public GetTreeFilesEndpoint(IQueryHandler<GetTreeFilesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>> getTreeFilesQueryHandler)
    {
        _getTreeFilesQueryHandler = getTreeFilesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Files.GET_TREE_FILES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the files of the path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the files.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetTreeFilesRequest request, CancellationToken cancellationToken)
    {
        Result<IEnumerable<FileSystemTreeNodeResponse>> result = await _getTreeFilesQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
