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

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// API endpoint for the <c>/path/api-validate</c> route.
/// </summary>
public class ValidatePathEndpoint : BaseEndpoint<ValidatePathRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ValidatePathEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Path.VALIDATE_PATH);
        DontAutoTag();
        Options(options => options.WithTags("Path"));
    }

    /// <summary>
    /// Validates the file system path identified by the request.
    /// </summary>
    /// <param name="request">The request containing the file system path to validate.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ValidatePathRequest request, CancellationToken cancellationToken)
    {
        PathValidDto response = await _apiHttpClient.GetAsync<PathValidDto>($"{ApiRoutes.Path.VALIDATE}?path={Uri.EscapeDataString(request.Path!)}", cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { isValid = response.IsValid });
    }
}
