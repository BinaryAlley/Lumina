#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;

/// <summary>
/// API endpoint for the <c>/books/{bookId}/reading/manifest</c> route.
/// </summary>
public class GetReadingManifestEndpoint : BaseEndpoint<GetReadingManifestRequest, IResult>
{
    private readonly IQueryHandler<GetReadingManifestQuery, Result<ReadingManifestResponse>> _getReadingManifestQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestEndpoint"/> class.
    /// </summary>
    /// <param name="getReadingManifestQueryHandler">Injected service for handling get reading manifest queries.</param>
    public GetReadingManifestEndpoint(IQueryHandler<GetReadingManifestQuery, Result<ReadingManifestResponse>> getReadingManifestQueryHandler)
    {
        _getReadingManifestQueryHandler = getReadingManifestQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_BOOK_READING_MANIFEST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the reading manifest of the book identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the book whose reading manifest is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetReadingManifestRequest request, CancellationToken cancellationToken)
    {
        Result<ReadingManifestResponse> result = await _getReadingManifestQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
