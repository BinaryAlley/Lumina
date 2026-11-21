#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Seed;

/// <summary>
/// Interface for the service for seeding initial data in the persistence medium.
/// </summary>
public interface IDataSeedService
{
    /// <summary>
    /// Sets up default authorization permissions in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Created>> SetDefaultAuthorizationPermissionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sets up default authorization roles in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Created>> SetDefaultAuthorizationRolesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Assigns admin role permissions to the admin user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Created>> SetAdminRolePermissionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Assigns admin role to the admin user.
    /// </summary>
    /// <param name="userId">The unique identifier of the admin user to receive the admin role.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Created>> SetAdminRoleToAdministratorAccount(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets up the default scheduled jobs, owned by the administrator account.
    /// </summary>
    /// <param name="adminUserId">The unique identifier of the administrator account that owns the default scheduled jobs.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Created>> SetDefaultScheduledJobsAsync(Guid adminUserId, CancellationToken cancellationToken);
}
