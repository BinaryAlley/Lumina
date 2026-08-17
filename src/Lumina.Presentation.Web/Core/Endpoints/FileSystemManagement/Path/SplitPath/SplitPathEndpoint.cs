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

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.SplitPath;

/// <summary>
/// API endpoint for the <c>/path/api-split</c> route.
/// </summary>
public class SplitPathEndpoint : BaseEndpoint<SplitPathRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public SplitPathEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Path.SPLIT_PATH);
        DontAutoTag();
        Options(options => options.WithTags("Path"));
    }

    /// <summary>
    /// Splits the file system path identified by the request into its segments.
    /// </summary>
    /// <param name="request">The request containing the file system path to split.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SplitPathRequest request, CancellationToken cancellationToken)
    {
        PathSegmentDto[] response = await _apiHttpClient.GetAsync<PathSegmentDto[]>($"{ApiRoutes.Path.SPLIT}?path={Uri.EscapeDataString(request.Path!)}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { pathSegments = response });
    }
}
