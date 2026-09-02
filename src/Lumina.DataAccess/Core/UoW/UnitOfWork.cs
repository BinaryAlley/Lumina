#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.DataAccess.Core.Repositories.Authorization;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.Repositories.Libraries;
using Lumina.DataAccess.Core.Repositories.MediaContributors;
using Lumina.DataAccess.Core.Repositories.Plugins;
using Lumina.DataAccess.Core.Repositories.Themes;
using Lumina.DataAccess.Core.Repositories.Users;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.UoW;

/// <summary>
/// Interaction boundary with the Data Access Layer.
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly LuminaDbContext _luminaDbContext;
    private IDbContextTransaction? _transaction;

    private IPermissionRepository? _permissionRepository;
    private IRolePermissionRepository? _rolePermissionRepository;
    private IRoleRepository? _roleRepository;
    private IUserRoleRepository? _userRoleRepository;
    private IBookRepository? _bookRepository;
    private IMediaContributorRepository? _mediaContributorRepository;
    private IDirectoryScanFingerprintRepository? _directoryScanFingerprintRepository;
    private ILibraryRepository? _libraryRepository;
    private ILibraryScanRepository? _libraryScanRepository;
    private ILibraryScanSnapshotRepository? _libraryScanSnapshotRepository;
    private ILibraryScanStagingResultsRepository? _libraryScanStagingResultsRepository;
    private ILibraryMetadataProviderConfigurationRepository? _libraryMetadataProviderConfigurationRepository;
    private IArtworkProviderConfigurationRepository? _artworkProviderConfigurationRepository;
    private ILibraryBookReaderConfigurationRepository? _libraryBookReaderConfigurationRepository;
    private IPluginRepository? _pluginRepository;
    private IThemeRepository? _themeRepository;
    private IUserRepository? _userRepository;
    private IUserSettingsRepository? _userSettingsRepository;

    /// <summary>
    /// Gets the permission repository.
    /// </summary>
    public IPermissionRepository PermissionRepository
    {
        get
        {
            _permissionRepository ??= new PermissionRepository(_luminaDbContext);
            return _permissionRepository;
        }
    }

    /// <summary>
    /// Gets the role permission repository.
    /// </summary>
    public IRolePermissionRepository RolePermissionRepository
    {
        get
        {
            _rolePermissionRepository ??= new RolePermissionRepository(_luminaDbContext);
            return _rolePermissionRepository;
        }
    }

    /// <summary>
    /// Gets the role repository.
    /// </summary>
    public IRoleRepository RoleRepository
    {
        get
        {
            _roleRepository ??= new RoleRepository(_luminaDbContext);
            return _roleRepository;
        }
    }

    /// <summary>
    /// Gets the user role repository.
    /// </summary>
    public IUserRoleRepository UserRoleRepository
    {
        get
        {
            _userRoleRepository ??= new UserRoleRepository(_luminaDbContext);
            return _userRoleRepository;
        }
    }

    /// <summary>
    /// Gets the book repository.
    /// </summary>
    public IBookRepository BookRepository
    {
        get
        {
            _bookRepository ??= new BookRepository(_luminaDbContext);
            return _bookRepository;
        }
    }

    public IMediaContributorRepository MediaContributorRepository
    {
        get
        {
            _mediaContributorRepository ??= new MediaContributorRepository(_luminaDbContext);
            return _mediaContributorRepository;
        }
    }

    /// <summary>
    /// Gets the directory scan fingerprint repository.
    /// </summary>
    public IDirectoryScanFingerprintRepository DirectoryScanFingerprintRepository
    {
        get
        {
            _directoryScanFingerprintRepository ??= new DirectoryScanFingerprintRepository(_luminaDbContext);
            return _directoryScanFingerprintRepository;
        }
    }

    /// <summary>
    /// Gets the library repository.
    /// </summary>
    public ILibraryRepository LibraryRepository
    {
        get
        {
            _libraryRepository ??= new LibraryRepository(_luminaDbContext);
            return _libraryRepository;
        }
    }

    /// <summary>
    /// Gets the library scan repository.
    /// </summary>
    public ILibraryScanRepository LibraryScanRepository
    {
        get
        {
            _libraryScanRepository ??= new LibraryScanRepository(_luminaDbContext);
            return _libraryScanRepository;
        }
    }

    /// <summary>
    /// Gets the library scan snapshot repository.
    /// </summary>
    public ILibraryScanSnapshotRepository LibraryScanSnapshotRepository
    {
        get
        {
            _libraryScanSnapshotRepository ??= new LibraryScanSnapshotRepository(_luminaDbContext);
            return _libraryScanSnapshotRepository;
        }
    }

    /// <summary>
    /// Gets the library scan staging results repository.
    /// </summary>
    public ILibraryScanStagingResultsRepository LibraryScanStagingResultsRepository
    {
        get
        {
            _libraryScanStagingResultsRepository ??= new LibraryScanStagingResultsRepository(_luminaDbContext);
            return _libraryScanStagingResultsRepository;
        }
    }

    /// <summary>
    /// Gets the library metadata provider configuration repository.
    /// </summary>
    public ILibraryMetadataProviderConfigurationRepository LibraryMetadataProviderConfigurationRepository
    {
        get
        {
            _libraryMetadataProviderConfigurationRepository ??= new LibraryMetadataProviderConfigurationRepository(_luminaDbContext);
            return _libraryMetadataProviderConfigurationRepository;
        }
    }

    /// <summary>
    /// Gets the library artwork provider configuration repository.
    /// </summary>
    public IArtworkProviderConfigurationRepository ArtworkProviderConfigurationRepository
    {
        get
        {
            _artworkProviderConfigurationRepository ??= new ArtworkProviderConfigurationRepository(_luminaDbContext);
            return _artworkProviderConfigurationRepository;
        }
    }

    /// <summary>
    /// Gets the library book reader configuration repository.
    /// </summary>
    public ILibraryBookReaderConfigurationRepository LibraryBookReaderConfigurationRepository
    {
        get
        {
            _libraryBookReaderConfigurationRepository ??= new LibraryBookReaderConfigurationRepository(_luminaDbContext);
            return _libraryBookReaderConfigurationRepository;
        }
    }

    /// <summary>
    /// Gets the plugin repository.
    /// </summary>
    public IPluginRepository PluginRepository
    {
        get
        {
            _pluginRepository ??= new PluginRepository(_luminaDbContext);
            return _pluginRepository;
        }
    }

    /// <summary>
    /// Gets the theme repository.
    /// </summary>
    public IThemeRepository ThemeRepository
    {
        get
        {
            _themeRepository ??= new ThemeRepository(_luminaDbContext);
            return _themeRepository;
        }
    }

    /// <summary>
    /// Gets the user repository.
    /// </summary>
    public IUserRepository UserRepository
    {
        get
        {
            _userRepository ??= new UserRepository(_luminaDbContext);
            return _userRepository;
        }
    }

    /// <summary>
    /// Gets the user settings repository.
    /// </summary>
    public IUserSettingsRepository UserSettingsRepository
    {
        get
        {
            _userSettingsRepository ??= new UserSettingsRepository(_luminaDbContext);
            return _userSettingsRepository;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public UnitOfWork(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Saves the changes made to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _luminaDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Detaches all the entities tracked by the unit of work, freeing the memory they occupy.
    /// </summary>
    public void ClearTrackedEntities()
    {
        _luminaDbContext.ChangeTracker.Clear();
    }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction = await _luminaDbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and its resources.
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _luminaDbContext.Dispose();
    }
}
