#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Tracking;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Handler for the command for cancelling the scan of a media library.
/// </summary>
public class CancelLibraryScanCommandHandler : IRequestHandler<CancelLibraryScanCommand, ErrorOr<Success>>
{
    private readonly IMediaLibraryScanningService _mediaLibraryScanningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanCommandHandler"/> class.
    /// </summary>
    /// <param name="mediaLibraryScanningService">Injected service for scanning media libraries.</param>
    public CancelLibraryScanCommandHandler(IMediaLibraryScanningService mediaLibraryScanningService)
    {
        _mediaLibraryScanningService = mediaLibraryScanningService;
    }

    /// <summary>
    /// Handles the command for cancelling the scan of a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async ValueTask<ErrorOr<Success>> Handle(CancelLibraryScanCommand request, CancellationToken cancellationToken)
    {
        _mediaLibraryScanningService.CancelScan(request.Id);
        return await ValueTask.FromResult(Result.Success);
    }
}
