#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.DeleteLibrary;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-item/{id}</c> route.
/// </summary>
public class DeleteLibraryEndpoint : BaseEndpoint<DeleteLibraryRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DeleteLibraryEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.DELETE);
        Routes(WebRoutes.LibraryManagement.DELETE_LIBRARY);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes a media library.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteLibraryRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.DeleteAsync(ApiRoutes.Libraries.DELETE_LIBRARY.Replace("{id}", request.Id.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
