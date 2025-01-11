#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
    /// <param name="adminId">The Id of the admin admin user who will own these permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Created>> SetDefaultAuthorizationPermissionsAsync(Guid adminId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets up default authorization roles in the system.
    /// </summary>
    /// <param name="userId">The Id of the admin user for whom roles will be set.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Created>> SetDefaultAuthorizationRolesAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Assigns admin role permissions to the admin user.
    /// </summary>
    /// <param name="userId">The Id of the admin user to receive admin role permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Created>> SetAdminRolePermissionsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Assigns admin role to the admin user.
    /// </summary>
    /// <param name="userId">The Id of the admin user to receive the admin role.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Created>> SetAdminRoleToAdministratorAccount(Guid userId, CancellationToken cancellationToken);
}
