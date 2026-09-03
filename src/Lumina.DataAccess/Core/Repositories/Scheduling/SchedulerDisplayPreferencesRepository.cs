#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Scheduling;

/// <summary>
/// Repository for the display preferences of the scheduler page.
/// </summary>
internal sealed class SchedulerDisplayPreferencesRepository : ISchedulerDisplayPreferencesRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerDisplayPreferencesRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public SchedulerDisplayPreferencesRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets the display preferences of the scheduler page of the user identified by <paramref name="userId"/>, if they exist.
    /// </summary>
    /// <param name="userId">The Id of the user whose display preferences are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the display preferences of the user, or an error.</returns>
    public async Task<Result<SchedulerDisplayPreferencesEntity?>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.SchedulerDisplayPreferences
            .FirstOrDefaultAsync(preferences => preferences.UserId == userId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts the display preferences of the scheduler page of a user, or updates them when they already exist.
    /// </summary>
    /// <param name="preferences">The display preferences to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpsertAsync(SchedulerDisplayPreferencesEntity preferences, CancellationToken cancellationToken)
    {
        SchedulerDisplayPreferencesEntity? existingPreferences = await _luminaDbContext.SchedulerDisplayPreferences
            .FirstOrDefaultAsync(repositoryPreferences => repositoryPreferences.UserId == preferences.UserId, cancellationToken).ConfigureAwait(false);
        if (existingPreferences is null)
            _luminaDbContext.SchedulerDisplayPreferences.Add(preferences);
        else
        {
            // Update only the mutable fields, preserving the Id and the ownership of the existing preferences.
            existingPreferences.JobTypeFilter = preferences.JobTypeFilter;
            existingPreferences.DisplayTimeSpan = preferences.DisplayTimeSpan;
            existingPreferences.DisplayTimeUnit = preferences.DisplayTimeUnit;
        }
        return Result.Updated;
    }
}
