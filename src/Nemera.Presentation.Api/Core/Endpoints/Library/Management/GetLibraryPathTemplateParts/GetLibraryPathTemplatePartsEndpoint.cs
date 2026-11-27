#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;

/// <summary>
/// API endpoint for the <c>/libraries/path-template-parts/{libraryType}</c> route.
/// </summary>
public class GetLibraryPathTemplatePartsEndpoint : BaseEndpoint<GetLibraryPathTemplatePartsRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryPathTemplatePartsQuery, Result<LibraryPathTemplateCatalogResponse>> _getLibraryPathTemplatePartsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryPathTemplatePartsEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryPathTemplatePartsQueryHandler">Injected service for handling get library path template parts queries.</param>
    public GetLibraryPathTemplatePartsEndpoint(IQueryHandler<GetLibraryPathTemplatePartsQuery, Result<LibraryPathTemplateCatalogResponse>> getLibraryPathTemplatePartsQueryHandler)
    {
        _getLibraryPathTemplatePartsQueryHandler = getLibraryPathTemplatePartsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_PATH_TEMPLATE_PARTS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the path parts available for a media library type.
    /// </summary>
    /// <param name="request">The request containing the media library type whose available path parts are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryPathTemplatePartsRequest request, CancellationToken cancellationToken)
    {
        Result<LibraryPathTemplateCatalogResponse> result = await _getLibraryPathTemplatePartsQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
