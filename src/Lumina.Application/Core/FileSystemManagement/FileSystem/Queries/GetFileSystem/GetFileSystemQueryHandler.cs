#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Handler for the query to get the file system type.
/// </summary>
public class GetFileSystemQueryHandler : IQueryHandler<GetFileSystemQuery, FileSystemTypeResponse>
{
    private readonly IPlatformContext _platformContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFileSystemQueryHandler"/> class.
    /// </summary>
    /// <param name="platformContextManager">Injected service for managing platform contexts.</param>
    public GetFileSystemQueryHandler(IPlatformContextManager platformContextManager)
    {
        _platformContext = platformContextManager.GetCurrentContext();
    }

    /// <summary>
    /// Gets the type of the file system.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The type of the file system.</returns>
    public Task<FileSystemTypeResponse> HandleAsync(GetFileSystemQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new FileSystemTypeResponse(_platformContext.Platform));
    }
}
