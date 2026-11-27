#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Users;

/// <summary>
/// Interface for the repository for user settings.
/// </summary>
public interface IUserSettingsRepository : IRepository<UserSettingsEntity>,
                                           IGetByIdRepositoryAction<UserSettingsEntity, Guid>,
                                           IInsertRepositoryAction<UserSettingsEntity>,
                                           IUpdateRepositoryAction<UserSettingsEntity>
{
    /// <summary>
    /// Gets the settings of the user identified by <paramref name="userId"/>, if they exist.
    /// </summary>
    /// <param name="userId">The Id of the user whose settings are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="UserSettingsEntity"/> of the user, or an error.</returns>
    Task<Result<UserSettingsEntity?>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
