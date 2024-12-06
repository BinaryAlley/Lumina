#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Users;

/// <summary>
/// Interface for the repository for users.
/// </summary>
public interface IUserRepository : IRepository<UserEntity>,
                                   IGetByIdRepositoryAction<UserEntity, Guid>,
                                   IGetAllRepositoryAction<UserEntity>,
                                   IInsertRepositoryAction<UserEntity>,
                                   IUpdateRepositoryAction<UserEntity>
{
    /// <summary>
    /// Gets a user identified by <paramref name="username"/> from the repository, if it exists.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="UserEntity"/> if found, or an error.</returns>
    Task<ErrorOr<UserEntity?>> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}
