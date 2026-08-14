#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.UoW;

/// <summary>
/// Interaction boundary with the Data Access Layer.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Gets the permission repository.
    /// </summary>
    IPermissionRepository PermissionRepository { get; }

    /// <summary>
    /// Gets the role permission repository.
    /// </summary>
    IRolePermissionRepository RolePermissionRepository { get; }

    /// <summary>
    /// Gets the role repository.
    /// </summary>
    IRoleRepository RoleRepository { get; }

    /// <summary>
    /// Gets the role user repository.
    /// </summary>
    IUserRoleRepository UserRoleRepository { get; }

    /// <summary>
    /// Gets the book repository.
    /// </summary>
    IBookRepository BookRepository { get; }

    /// <summary>
    /// Gets the directory scan fingerprint repository.
    /// </summary>
    IDirectoryScanFingerprintRepository DirectoryScanFingerprintRepository { get; }

    /// <summary>
    /// Gets the library repository.
    /// </summary>
    ILibraryRepository LibraryRepository { get; }

    /// <summary>
    /// Gets the library scan repository.
    /// </summary>
    ILibraryScanRepository LibraryScanRepository { get; }

    /// <summary>
    /// Gets the library scan snapshot repository.
    /// </summary>
    ILibraryScanSnapshotRepository LibraryScanSnapshotRepository { get; }

    /// <summary>
    /// Gets the library scan staging results repository.
    /// </summary>
    ILibraryScanStagingResultsRepository LibraryScanStagingResultsRepository { get; }

    /// <summary>
    /// Gets the library metadata provider configuration repository.
    /// </summary>
    ILibraryMetadataProviderConfigurationRepository LibraryMetadataProviderConfigurationRepository { get; }

    /// <summary>
    /// Gets the plugin repository.
    /// </summary>
    IPluginRepository PluginRepository { get; }

    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IUserRepository UserRepository { get; }

    /// <summary>
    /// Saves all changes made to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken);


    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
