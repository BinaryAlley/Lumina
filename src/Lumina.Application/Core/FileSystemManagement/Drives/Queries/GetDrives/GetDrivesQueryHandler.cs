#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;

/// <summary>
/// Handler for the query to get all drives.
/// </summary>
public class GetDrivesQueryHandler : IQueryHandler<GetDrivesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>>
{
    private readonly IDriveService _driveService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesQueryHandler"/> class.
    /// </summary>
    /// <param name="driveService">Injected service for handling file system drives.</param>
    public GetDrivesQueryHandler(IDriveService driveService)
    {
        _driveService = driveService;
    }

    /// <summary>
    /// Gets the list of file system drives.
    /// </summary>
    /// <param name="query">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="FileSystemTreeNodeResponse"/>, or an error message.
    /// </returns>
    public Task<Result<IEnumerable<FileSystemTreeNodeResponse>>> HandleAsync(GetDrivesQuery query, CancellationToken cancellationToken)
    {
        Result<IEnumerable<FileSystemItem>> getDrivesResult = _driveService.GetDrives();
        return Task.FromResult(getDrivesResult.Match(values => Result.From(values.ToTreeNodeResponses()), errors => errors));
    }
}
