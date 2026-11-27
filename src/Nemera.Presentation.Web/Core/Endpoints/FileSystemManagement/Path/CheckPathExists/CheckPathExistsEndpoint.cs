#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.FileSystemManagement;
using Nemera.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;

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
        PathExistsDto response = await _apiHttpClient.GetAsync<PathExistsDto>($"{ApiRoutes.Path.CHECK_PATH_EXISTS}?path={Uri.EscapeDataString(request.Path!)}&shouldIncludeHiddenElements={request.ShouldIncludeHiddenElements}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { exists = response.Exists });
    }
}
