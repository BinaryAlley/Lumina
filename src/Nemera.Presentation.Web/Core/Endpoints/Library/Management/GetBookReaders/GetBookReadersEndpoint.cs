#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetBookReaders;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-get-book-readers/{libraryId}</c> route.
/// </summary>
public class GetBookReadersEndpoint : BaseEndpoint<GetBookReadersRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookReadersEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetBookReadersEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryManagement.GET_BOOK_READERS);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
    }

    /// <summary>
    /// Retrieves the book readers of a media library.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose book readers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookReadersRequest request, CancellationToken cancellationToken)
    {
        LibraryBookReaderDto[] response = await _apiHttpClient.GetAsync<LibraryBookReaderDto[]>(ApiRoutes.Libraries.GET_LIBRARY_BOOK_READERS.Replace("{libraryId}", request.LibraryId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
