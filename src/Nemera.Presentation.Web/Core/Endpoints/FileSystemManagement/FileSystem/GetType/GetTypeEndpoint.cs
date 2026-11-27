#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.FileSystemManagement;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.FileSystem.GetType;

/// <summary>
/// API endpoint for the <c>/file-system/api-get-type</c> route.
/// </summary>
public class GetTypeEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTypeEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetTypeEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.FileSystem.GET_TYPE);
        DontAutoTag();
        Options(options => options.WithTags("FileSystem"));
    }

    /// <summary>
    /// Retrieves the file system platform type.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        FileSystemTypeDto response = await _apiHttpClient.GetAsync<FileSystemTypeDto>(ApiRoutes.FileSystem.GET_TYPES, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { platformType = response.PlatformType.ToString() });
    }
}
