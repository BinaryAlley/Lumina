#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.SetLibraryBookReaderEnabled;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/book-readers/{pluginId}/enabled</c> route.
/// </summary>
public class SetLibraryBookReaderEnabledEndpoint : BaseEndpoint<SetLibraryBookReaderEnabledRequest, IResult>
{
    private readonly ICommandHandler<SetLibraryBookReaderEnabledCommand, Result<Success>> _setLibraryBookReaderEnabledCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryBookReaderEnabledEndpoint"/> class.
    /// </summary>
    /// <param name="setLibraryBookReaderEnabledCommandHandler">Injected service for handling set library book reader enabled commands.</param>
    public SetLibraryBookReaderEnabledEndpoint(ICommandHandler<SetLibraryBookReaderEnabledCommand, Result<Success>> setLibraryBookReaderEnabledCommandHandler)
    {
        _setLibraryBookReaderEnabledCommandHandler = setLibraryBookReaderEnabledCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);        
        Routes(ApiRoutes.Libraries.SET_LIBRARY_BOOK_READER_ENABLED);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Enables or disables the book reader identified by <paramref name="request"/> for the media library.
    /// </summary>
    /// <param name="request">The request containing the Ids of the media library and of the book reader.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SetLibraryBookReaderEnabledRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _setLibraryBookReaderEnabledCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
