#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-get-path-template-parts/{libraryType}</c> route.
/// </summary>
public class GetLibraryPathTemplatePartsEndpoint : BaseEndpoint<GetLibraryPathTemplatePartsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryPathTemplatePartsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetLibraryPathTemplatePartsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryManagement.GET_LIBRARY_PATH_TEMPLATE_PARTS);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
    }

    /// <summary>
    /// Retrieves the path parts available for a media library type.
    /// </summary>
    /// <param name="request">The request containing the media library type whose available path parts are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryPathTemplatePartsRequest request, CancellationToken cancellationToken)
    {
        LibraryPathTemplateCatalogDto response = await _apiHttpClient.GetAsync<LibraryPathTemplateCatalogDto>(ApiRoutes.Libraries.GET_LIBRARY_PATH_TEMPLATE_PARTS.Replace("{libraryType}", request.LibraryType), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
