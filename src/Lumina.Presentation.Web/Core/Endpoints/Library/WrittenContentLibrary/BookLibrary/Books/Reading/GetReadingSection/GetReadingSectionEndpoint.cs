#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-section</c> route.
/// </summary>
public class GetReadingSectionEndpoint : BaseEndpoint<GetBookReadingSectionRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetReadingSectionEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Books.GET_READING_SECTION);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Retrieves the content of a reading section of a book.
    /// </summary>
    /// <param name="request">The request containing the Id of the book and the location reference of the reading section.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookReadingSectionRequest request, CancellationToken cancellationToken)
    {
        ReadingSectionDto response = await _apiHttpClient.GetAsync<ReadingSectionDto>(ApiRoutes.Books.GET_BOOK_READING_SECTION
            .Replace("{bookId}", request.BookId.ToString())
            .Replace("{locationRef}", Uri.EscapeDataString(request.LocationRef)), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
