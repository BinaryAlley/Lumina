#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Scheduling;

/// <summary>
/// Interface for the repository for the display preferences of the scheduler page.
/// </summary>
public interface ISchedulerDisplayPreferencesRepository : IRepository<SchedulerDisplayPreferencesEntity>
{
    /// <summary>
    /// Gets the display preferences of the scheduler page of the user identified by <paramref name="userId"/>, if they exist.
    /// </summary>
    /// <param name="userId">The Id of the user whose display preferences are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the display preferences of the user, or an error.</returns>
    Task<Result<SchedulerDisplayPreferencesEntity?>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the display preferences of the scheduler page of a user, or updates them when they already exist.
    /// </summary>
    /// <param name="preferences">The display preferences to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> UpsertAsync(SchedulerDisplayPreferencesEntity preferences, CancellationToken cancellationToken);
}
