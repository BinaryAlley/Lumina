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

namespace Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.Drives.GetDrives;

/// <summary>
/// API endpoint for the <c>/drives/api-get-drives</c> route.
/// </summary>
public class GetDrivesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetDrivesEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Drives.GET_DRIVES);
        DontAutoTag();
        Options(options => options.WithTags("Drives"));
    }

    /// <summary>
    /// Retrieves the file system drives.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        FileSystemTreeNodeDto[] response = await _apiHttpClient.GetAsync<FileSystemTreeNodeDto[]>(ApiRoutes.Drives.GET_DRIVES, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { drives = response });
    }
}
