#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Users;

/// <summary>
/// Repository for user settings.
/// </summary>
internal sealed class UserSettingsRepository : IUserSettingsRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingsRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public UserSettingsRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds the settings of a user.
    /// </summary>
    /// <param name="data">The user settings to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(UserSettingsEntity data, CancellationToken cancellationToken)
    {
        bool doesSettingsExist = await _luminaDbContext.UserSettings.AnyAsync(settings => settings.UserId == data.UserId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesSettingsExist)
            return Errors.UserSettings.UserSettingsAlreadyExists;

        _luminaDbContext.UserSettings.Add(data);
        return Result.Created;
    }

    /// <summary>
    /// Gets the settings identified by <paramref name="id"/>, if they exist.
    /// </summary>
    /// <param name="id">The Id of the user settings to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="UserSettingsEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<UserSettingsEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<UserSettingsEntity> query = _luminaDbContext.UserSettings;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(settings => settings.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the settings of the user identified by <paramref name="userId"/>, if they exist.
    /// </summary>
    /// <param name="userId">The Id of the user whose settings are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="UserSettingsEntity"/> of the user, or an error.</returns>
    public async Task<Result<UserSettingsEntity?>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.UserSettings
            .FirstOrDefaultAsync(settings => settings.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the settings of a user.
    /// </summary>
    /// <param name="data">The user settings to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(UserSettingsEntity data, CancellationToken cancellationToken)
    {
        UserSettingsEntity? foundSettings = await _luminaDbContext.UserSettings
            .FirstOrDefaultAsync(settings => settings.Id == data.Id, cancellationToken)
            .ConfigureAwait(false);
        if (foundSettings is null)
            return Errors.UserSettings.UserSettingsNotFound;

        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundSettings, data);
        return Result.Updated;
    }
}
