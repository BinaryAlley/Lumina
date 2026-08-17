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

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;

/// <summary>
/// API endpoint for the <c>/path/api-check-path-exists</c> route.
/// </summary>
public class CheckPathExistsEndpoint : BaseEndpoint<CheckPathExistsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public CheckPathExistsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Path.CHECK_PATH_EXISTS);
        DontAutoTag();
        Options(options => options.WithTags("Path"));
    }

    /// <summary>
    /// Checks whether the file system path identified by the request exists.
    /// </summary>
    /// <param name="request">The request containing the file system path whose existence is checked.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CheckPathExistsRequest request, CancellationToken cancellationToken)
    {
        PathExistsDto response = await _apiHttpClient.GetAsync<PathExistsDto>($"{ApiRoutes.Path.CHECK_PATH_EXISTS}?path={Uri.EscapeDataString(request.Path!)}&includeHiddenElements={request.IncludeHiddenElements}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { exists = response.Exists });
    }
}
