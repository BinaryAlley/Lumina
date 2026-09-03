#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ApplicationAuthorizationService = Lumina.Application.Common.Infrastructure.Authorization.IAuthorizationService;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Notifications;

/// <summary>
/// SignalR hub for broadcasting the state of the scheduled jobs to the connected administrator clients.
/// </summary>
[Authorize]
public class ScheduledJobsHub : Hub<IScheduledJobsClient>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobsHub"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Injected factory used for creating scopes in which services are requested.</param>
    public ScheduledJobsHub(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Verifies that the connecting client belongs to an administrator account, and rejects the connection otherwise.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task OnConnectedAsync()
    {
        string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId) || !await IsAdministratorAsync(userId).ConfigureAwait(false))
        {
            Context.Abort();
            return;
        }
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the user identified by <paramref name="userId"/> is an administrator.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to check.</param>
    /// <returns><see langword="true"/> when the user is an administrator, <see langword="false"/> otherwise.</returns>
    private async Task<bool> IsAdministratorAsync(Guid userId)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        ApplicationAuthorizationService authorizationService = scope.ServiceProvider.GetRequiredService<ApplicationAuthorizationService>();
        return await authorizationService.IsInRoleAsync(userId, "Admin", CancellationToken.None).ConfigureAwait(false);
    }
}
