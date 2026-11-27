#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;
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

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeBookReaders;

/// <summary>
/// API endpoint for the <c>/libraries/book-readers/{libraryType}</c> route.
/// </summary>
public class GetLibraryTypeBookReadersEndpoint : BaseEndpoint<GetLibraryTypeBookReadersRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryTypeBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>> _getLibraryTypeBookReadersQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryTypeBookReadersQueryHandler">Injected service for handling get library type book readers queries.</param>
    public GetLibraryTypeBookReadersEndpoint(IQueryHandler<GetLibraryTypeBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>> getLibraryTypeBookReadersQueryHandler)
    {
        _getLibraryTypeBookReadersQueryHandler = getLibraryTypeBookReadersQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_TYPE_BOOK_READERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the book readers available for the media library type provided in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the media library type whose available book readers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryTypeBookReadersRequest request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _getLibraryTypeBookReadersQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
