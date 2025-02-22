#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Progress;

/// <summary>
/// SignalR hub for managing real-time media library scan progress updates.
/// </summary>
public class MediaLibraryScanProgressHub : Hub
{
    /// <summary>
    /// Subscribes the current connection to scan progress notifications for a specific scan operation.
    /// </summary>
    /// <param name="scanId">Unique identifier of the scan operation.</param>
    /// <param name="userId">Unique identifier of the requesting user.</param>
    public async Task SubscribeToScan(Guid scanId, Guid userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{scanId}-{userId}");
    }
}
