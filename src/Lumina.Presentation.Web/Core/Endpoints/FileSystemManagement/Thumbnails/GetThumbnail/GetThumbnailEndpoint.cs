#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Thumbnails;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;

/// <summary>
/// API endpoint for the <c>/thumbnails/api-get-thumbnail</c> route.
/// </summary>
public class GetThumbnailEndpoint : BaseEndpoint<GetThumbnailRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetThumbnailEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Thumbnails.GET_THUMBNAIL);
        DontAutoTag();
        Options(options => options.WithTags("Thumbnails"));
    }

    /// <summary>
    /// Retrieves the thumbnail of the file located at the path identified by the request.
    /// </summary>
    /// <param name="request">The request containing the file system path of the file for which to get the thumbnail.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThumbnailRequest request, CancellationToken cancellationToken)
    {
        // the thumbnail is proxied through the Web application, so that the request carries the authentication of the current user
        BlobDataDto blob = await _apiHttpClient.GetBlobAsync($"{ApiRoutes.Thumbnails.GET_THUMBNAIL}?path={Uri.EscapeDataString(request.Path!)}&quality={request.Quality}", cancellationToken).ConfigureAwait(false);
        return Results.File(blob.Data, blob.ContentType);
    }
}
