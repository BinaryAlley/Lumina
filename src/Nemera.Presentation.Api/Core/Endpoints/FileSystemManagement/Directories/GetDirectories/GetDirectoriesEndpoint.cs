#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// API endpoint for the <c>/directories/get-directories</c> route.
/// </summary>
public class GetDirectoriesEndpoint : BaseEndpoint<GetDirectoriesRequest, IResult>
{
    private readonly IQueryHandler<GetDirectoriesQuery, Result<IEnumerable<DirectoryResponse>>> _getDirectoriesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpoint"/> class.
    /// </summary>
    /// <param name="getDirectoriesQueryHandler">Injected service for handling get directories queries.</param>
    public GetDirectoriesEndpoint(IQueryHandler<GetDirectoriesQuery, Result<IEnumerable<DirectoryResponse>>> getDirectoriesQueryHandler)
    {
        _getDirectoriesQueryHandler = getDirectoriesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Directories.GET_DIRECTORIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the directories of the path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the directories.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetDirectoriesRequest request, CancellationToken cancellationToken)
    {
        Result<IEnumerable<DirectoryResponse>> result = await _getDirectoriesQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
