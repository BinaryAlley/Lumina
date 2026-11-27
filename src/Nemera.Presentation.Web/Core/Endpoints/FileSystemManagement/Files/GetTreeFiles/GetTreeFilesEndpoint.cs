#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.FileSystemManagement;
using Nemera.Presentation.Web.Common.Requests.FileSystemManagement.Files;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.Files.GetTreeFiles;

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
        FileSystemTreeNodeDto[] response = await _apiHttpClient.GetAsync<FileSystemTreeNodeDto[]>($"{ApiRoutes.Files.GET_TREE_FILES}?path={Uri.EscapeDataString(request.Path!)}&shouldIncludeHiddenElements={request.ShouldIncludeHiddenElements}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
