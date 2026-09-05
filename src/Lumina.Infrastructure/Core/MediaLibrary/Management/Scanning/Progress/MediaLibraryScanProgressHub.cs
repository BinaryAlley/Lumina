#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Progress;

/// <summary>
/// SignalR hub for managing real-time media library scan progress updates.
/// </summary>
[Authorize]
public class MediaLibraryScanProgressHub : Hub
{
    /// <summary>
    /// Subscribes the current connection to scan progress notifications for a specific scan operation.
    /// </summary>
    /// <param name="scanId">Unique identifier of the scan operation.</param>
    public async Task SubscribeToScan(Guid scanId)
    {
        // The user is never taken from the client, because any client could otherwise subscribe to the progress of a scan by claiming another user's identifier.
        // The user is taken from the authenticated connection instead.
        string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return;
        await Groups.AddToGroupAsync(Context.ConnectionId, MediaLibraryScanCompositeId.Create(ScanId.Create(scanId), UserId.Create(userId)).ToString());
    }
}
