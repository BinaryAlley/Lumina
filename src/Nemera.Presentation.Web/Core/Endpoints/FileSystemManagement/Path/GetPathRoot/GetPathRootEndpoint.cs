#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.GetPathRoot;

/// <summary>
/// API endpoint for the <c>/path/api-get-path-root</c> route.
/// </summary>
public class GetPathRootEndpoint : BaseEndpoint<GetPathRootRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetPathRootEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Path.GET_PATH_ROOT);
        DontAutoTag();
        Options(options => options.WithTags("Path"));
    }

    /// <summary>
    /// Retrieves the root of the file system path identified by the request.
    /// </summary>
    /// <param name="request">The request containing the file system path whose root is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPathRootRequest request, CancellationToken cancellationToken)
    {
        PathSegmentDto response = await _apiHttpClient.GetAsync<PathSegmentDto>($"{ApiRoutes.Path.GET_PATH_ROOT}?path={Uri.EscapeDataString(request.Path!)}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { root = response });
    }
}
