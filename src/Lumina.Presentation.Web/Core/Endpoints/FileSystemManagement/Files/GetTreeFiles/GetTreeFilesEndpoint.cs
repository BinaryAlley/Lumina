#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Files;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Files.GetTreeFiles;

/// <summary>
/// API endpoint for the <c>/files/api-get-tree-files</c> route.
/// </summary>
public class GetTreeFilesEndpoint : BaseEndpoint<GetTreeFilesRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetTreeFilesEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Files.GET_TREE_FILES);
        DontAutoTag();
        Options(options => options.WithTags("Files"));
    }

    /// <summary>
    /// Retrieves the file system tree of the file system path identified by the request.
    /// </summary>
    /// <param name="request">The request containing the file system path whose file system tree is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetTreeFilesRequest request, CancellationToken cancellationToken)
    {
        FileSystemTreeNodeDto[] response = await _apiHttpClient.GetAsync<FileSystemTreeNodeDto[]>($"{ApiRoutes.Files.GET_TREE_FILES}?path={Uri.EscapeDataString(request.Path!)}&includeHiddenElements={request.IncludeHiddenElements}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
