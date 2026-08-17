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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.EditLibrary;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/item/{id}</c> route.
/// </summary>
public class EditLibraryViewEndpoint : BaseEndpoint<EditLibraryRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditLibraryViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public EditLibraryViewEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryManagement.EDIT_LIBRARY);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
    }

    /// <summary>
    /// Displays the media library editing view.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library to edit.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EditLibraryRequest request, CancellationToken cancellationToken)
    {
        LibraryDto library = await _apiHttpClient.GetAsync<LibraryDto>(ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", request.Id.ToString()), cancellationToken).ConfigureAwait(false);
        return View("/Core/Views/Library/Management/Item.cshtml", library);
    }
}
