#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Drives.GetDrives;

/// <summary>
/// API endpoint for the <c>/drives/get-drives</c> route.
/// </summary>
public class GetDrivesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetDrivesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>> _getDrivesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpoint"/> class.
    /// </summary>
    /// <param name="getDrivesQueryHandler">Injected service for handling get drives queries.</param>
    public GetDrivesEndpoint(IQueryHandler<GetDrivesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>> getDrivesQueryHandler)
    {
        _getDrivesQueryHandler = getDrivesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Drives.GET_DRIVES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of file system drives.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<IEnumerable<FileSystemTreeNodeResponse>> result = await _getDrivesQueryHandler.HandleAsync(new GetDrivesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
