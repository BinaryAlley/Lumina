#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.FileSystem.GetType;

/// <summary>
/// API endpoint for the <c>/file-system/get-type</c> route.
/// </summary>
public class GetTypeEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetFileSystemQuery, FileSystemTypeResponse> _getFileSystemQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTypeEndpoint"/> class.
    /// </summary>
    /// <param name="getFileSystemQueryHandler">Injected service for handling get file system queries.</param>
    public GetTypeEndpoint(IQueryHandler<GetFileSystemQuery, FileSystemTypeResponse> getFileSystemQueryHandler)
    {
        _getFileSystemQueryHandler = getFileSystemQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.FileSystem.GET_TYPES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the type of the file system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        FileSystemTypeResponse platformType = await _getFileSystemQueryHandler.HandleAsync(new GetFileSystemQuery(), cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(platformType);
    }
}
