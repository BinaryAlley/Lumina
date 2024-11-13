#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetDirectoryTree;

/// <summary>
/// API endpoint for the <c>/directories/get-directory-tree</c> route.
/// </summary>
public class GetDirectoryTreeEndpoint : BaseEndpoint<GetDirectoryTreeRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetDirectoryTreeEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Directories.GET_DIRECTORY_TREE);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the tree of expanded directories leading up to the path stored in <paramref name="request"/>, with the additional list of drives, and children of the last child directory.
    /// </summary>
    /// <param name="request">The request containing the file system path for for which to get the directory tree.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetDirectoryTreeRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
