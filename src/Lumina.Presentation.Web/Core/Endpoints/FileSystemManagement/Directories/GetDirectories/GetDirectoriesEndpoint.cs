#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Directories;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// API endpoint for the <c>/directories/api-get-directories</c> route.
/// </summary>
public class GetDirectoriesEndpoint : BaseEndpoint<GetDirectoriesRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetDirectoriesEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Directories.GET_DIRECTORIES);
        DontAutoTag();
        Options(options => options.WithTags("Directories"));
    }

    /// <summary>
    /// Retrieves the directories of the file system path identified by the request.
    /// </summary>
    /// <param name="request">The request containing the file system path whose directories are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetDirectoriesRequest request, CancellationToken cancellationToken)
    {
        DirectoryDto[] response = await _apiHttpClient.GetAsync<DirectoryDto[]>($"{ApiRoutes.Directories.GET_DIRECTORIES}?path={Uri.EscapeDataString(request.Path!)}&includeHiddenElements={request.IncludeHiddenElements}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
