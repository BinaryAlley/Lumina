#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Handler for the query to get the file system type.
/// </summary>
public class GetFileSystemQueryHandler : IRequestHandler<GetFileSystemQuery, FileSystemTypeResponse>
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
    /// <param name="request">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The type of the file system.</returns>
    public ValueTask<FileSystemTypeResponse> Handle(GetFileSystemQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new FileSystemTypeResponse(_platformContext.Platform));
    }
}
