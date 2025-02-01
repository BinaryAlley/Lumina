#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Tracking;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

/// <summary>
/// Handler for the command for cancelling the scan of all media libraries.
/// </summary>
public class CancelLibrariesScanCommandHandler : IRequestHandler<CancelLibrariesScanCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibrariesScanTracker _librariesScanTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanCommandHandler"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="mediaLibraryScanningService">Injected service for scanning media libraries.</param>
    public CancelLibrariesScanCommandHandler(ICurrentUserService currentUserService, IMediaLibrariesScanTracker librariesScanTracker)
    {
        _currentUserService = currentUserService;
        _librariesScanTracker = librariesScanTracker;
    }

    /// <summary>
    /// Handles the command for cancelling the scan of all media libraries.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async ValueTask<ErrorOr<Success>> Handle(CancelLibrariesScanCommand request, CancellationToken cancellationToken)
    {
        _librariesScanTracker.CancelUserScans(_currentUserService.UserId!.Value);
        return await ValueTask.FromResult(Result.Success);
    }
}
