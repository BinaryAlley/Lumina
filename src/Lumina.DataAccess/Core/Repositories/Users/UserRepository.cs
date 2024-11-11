#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Users;

/// <summary>
/// Repository for users.
/// </summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public UserRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new user.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(UserEntity user, CancellationToken cancellationToken)
    {
        bool userExists = await _luminaDbContext.Users.AnyAsync(repositoryUser => repositoryUser.Id == user.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (userExists)
            return Errors.Users.UserAlreadyExists;

        _luminaDbContext.Users.Add(user);
        return Result.Created;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="UserEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<UserEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Users.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a username identified by <paramref name="username"/> from the repository, if it exists.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="UserEntity"/> if found, or an error.</returns>
    public async Task<ErrorOr<UserEntity?>> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Users.FirstOrDefaultAsync(user => user.Username == username, cancellationToken).ConfigureAwait(false);
    }
}
