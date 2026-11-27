#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryBookReaders;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/book-readers</c> route.
/// </summary>
public class GetLibraryBookReadersEndpoint : BaseEndpoint<GetLibraryBookReadersRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>> _getLibraryBookReadersQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryBookReadersEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryBookReadersQueryHandler">Injected service for handling get library book readers queries.</param>
    public GetLibraryBookReadersEndpoint(IQueryHandler<GetLibraryBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>> getLibraryBookReadersQueryHandler)
    {
        _getLibraryBookReadersQueryHandler = getLibraryBookReadersQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_BOOK_READERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the book readers configured for the media library identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose book readers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryBookReadersRequest request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _getLibraryBookReadersQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
