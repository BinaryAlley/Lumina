#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.SaveLibrary;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-item</c> route.
/// </summary>
public class SaveLibraryEndpoint : BaseEndpoint<LibraryDto, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public SaveLibraryEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.LibraryManagement.SAVE_LIBRARY);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
        EnableAntiforgery();
        Policies(AuthorizationPolicies.REQUIRE_CREATE_LIBRARIES_PERMISSION);
    }

    /// <summary>
    /// Creates or updates a media library.
    /// </summary>
    /// <param name="request">The request containing the details of the media library to create or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(LibraryDto request, CancellationToken cancellationToken)
    {
        LibraryDto response = request.Id.HasValue
            ? await _apiHttpClient.PutAsync<LibraryDto, LibraryDto>(ApiRoutes.Libraries.UPDATE_LIBRARY.Replace("{id}", request.Id.Value.ToString()), request, cancellationToken).ConfigureAwait(false)
            : await _apiHttpClient.PostAsync<LibraryDto, LibraryDto>(ApiRoutes.Libraries.ADD_LIBRARY, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
