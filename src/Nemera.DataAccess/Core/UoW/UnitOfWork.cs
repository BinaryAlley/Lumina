#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Repositories.Authorization;
using Nemera.Application.Common.DataAccess.Repositories.BookLibrary;
using Nemera.Application.Common.DataAccess.Repositories.Favorites;
using Nemera.Application.Common.DataAccess.Repositories.MediaConsumption;
using Nemera.Application.Common.DataAccess.Repositories.MediaContributors;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.Application.Common.DataAccess.Repositories.MusicLibrary;
using Nemera.Application.Common.DataAccess.Repositories.Playlists;
using Nemera.Application.Common.DataAccess.Repositories.Plugins;
using Nemera.Application.Common.DataAccess.Repositories.Scheduling;
using Nemera.Application.Common.DataAccess.Repositories.Themes;
using Nemera.Application.Common.DataAccess.Repositories.Users;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Time;
using Nemera.DataAccess.Core.Repositories.Authorization;
using Nemera.DataAccess.Core.Repositories.BookLibrary;
using Nemera.DataAccess.Core.Repositories.Favorites;
using Nemera.DataAccess.Core.Repositories.Libraries;
using Nemera.DataAccess.Core.Repositories.MediaConsumption;
using Nemera.DataAccess.Core.Repositories.MediaContributors;
using Nemera.DataAccess.Core.Repositories.MusicLibrary;
using Nemera.DataAccess.Core.Repositories.Playlists;
using Nemera.DataAccess.Core.Repositories.Plugins;
using Nemera.DataAccess.Core.Repositories.Scheduling;
using Nemera.DataAccess.Core.Repositories.Themes;
using Nemera.DataAccess.Core.Repositories.Users;
using Nemera.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.Core.UoW;

/// <summary>
/// Interaction boundary with the Data Access Layer.
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly NemeraDbContext _nemeraDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private IDbContextTransaction? _transaction;

    private IPermissionRepository? _permissionRepository;
    private IRolePermissionRepository? _rolePermissionRepository;
    private IRoleRepository? _roleRepository;
    private IUserRoleRepository? _userRoleRepository;
    private IBookRepository? _bookRepository;
    private IArtistRepository? _artistRepository;
    private IAlbumRepository? _albumRepository;
    private ITrackRepository? _trackRepository;
    private IMusicArtworkRepository? _musicArtworkRepository;
    private IMusicLibraryScanItemMetadataRepository? _musicLibraryScanItemMetadataRepository;
    private IMediaContributorRepository? _mediaContributorRepository;
    private IDirectoryScanFingerprintRepository? _directoryScanFingerprintRepository;
    private ILibraryRepository? _libraryRepository;
    private ILibraryScanRepository? _libraryScanRepository;
    private ILibraryScanSnapshotRepository? _libraryScanSnapshotRepository;
    private ILibraryScanStagingResultsRepository? _libraryScanStagingResultsRepository;
    private ILibraryMetadataProviderConfigurationRepository? _libraryMetadataProviderConfigurationRepository;
    private IArtworkProviderConfigurationRepository? _artworkProviderConfigurationRepository;
    private ILibraryBookReaderConfigurationRepository? _libraryBookReaderConfigurationRepository;
    private IScheduledJobRepository? _scheduledJobRepository;
    private IScheduledJobExecutionRepository? _scheduledJobExecutionRepository;
    private ISchedulerDisplayPreferencesRepository? _schedulerDisplayPreferencesRepository;
    private IPluginRepository? _pluginRepository;
    private IThemeRepository? _themeRepository;
    private IUserRepository? _userRepository;
    private IUserSettingsRepository? _userSettingsRepository;
    private IPlaylistRepository? _playlistRepository;
    private IFavoriteRepository? _favoriteRepository;
    private IMediaConsumptionRecordRepository? _mediaConsumptionRecordRepository;
    private IMediaResumePointRepository? _mediaResumePointRepository;
    private IConsumptionStatisticsRepository? _consumptionStatisticsRepository;

    /// <summary>
    /// Gets the permission repository.
    /// </summary>
    public IPermissionRepository PermissionRepository
    {
        get
        {
            _permissionRepository ??= new PermissionRepository(_nemeraDbContext);
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
            _rolePermissionRepository ??= new RolePermissionRepository(_nemeraDbContext);
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
            _roleRepository ??= new RoleRepository(_nemeraDbContext);
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
            _userRoleRepository ??= new UserRoleRepository(_nemeraDbContext);
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
            _bookRepository ??= new BookRepository(_nemeraDbContext, _dateTimeProvider);
            return _bookRepository;
        }
    }

    /// <summary>
    /// Gets the artist repository.
    /// </summary>
    public IArtistRepository ArtistRepository
    {
        get
        {
            _artistRepository ??= new ArtistRepository(_nemeraDbContext, AlbumRepository, TrackRepository, _dateTimeProvider);
            return _artistRepository;
        }
    }

    /// <summary>
    /// Gets the album repository.
    /// </summary>
    public IAlbumRepository AlbumRepository
    {
        get
        {
            _albumRepository ??= new AlbumRepository(_nemeraDbContext, TrackRepository, _dateTimeProvider);
            return _albumRepository;
        }
    }

    /// <summary>
    /// Gets the track repository.
    /// </summary>
    public ITrackRepository TrackRepository
    {
        get
        {
            _trackRepository ??= new TrackRepository(_nemeraDbContext, _dateTimeProvider);
            return _trackRepository;
        }
    }

    /// <summary>
    /// Gets the repository for the artwork of the music library items.
    /// </summary>
    public IMusicArtworkRepository MusicArtworkRepository
    {
        get
        {
            _musicArtworkRepository ??= new MusicArtworkRepository(_nemeraDbContext);
            return _musicArtworkRepository;
        }
    }

    /// <summary>
    /// Gets the repository for the music metadata staged during a media library scan.
    /// </summary>
    public IMusicLibraryScanItemMetadataRepository MusicLibraryScanItemMetadataRepository
    {
        get
        {
            _musicLibraryScanItemMetadataRepository ??= new MusicLibraryScanItemMetadataRepository(_nemeraDbContext);
            return _musicLibraryScanItemMetadataRepository;
        }
    }

    /// <summary>
    /// Gets the media contributor repository.
    /// </summary>
    public IMediaContributorRepository MediaContributorRepository
    {
        get
        {
            _mediaContributorRepository ??= new MediaContributorRepository(_nemeraDbContext);
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
            _directoryScanFingerprintRepository ??= new DirectoryScanFingerprintRepository(_nemeraDbContext);
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
            _libraryRepository ??= new LibraryRepository(_nemeraDbContext);
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
            _libraryScanRepository ??= new LibraryScanRepository(_nemeraDbContext);
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
            _libraryScanSnapshotRepository ??= new LibraryScanSnapshotRepository(_nemeraDbContext);
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
            _libraryScanStagingResultsRepository ??= new LibraryScanStagingResultsRepository(_nemeraDbContext);
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
            _libraryMetadataProviderConfigurationRepository ??= new LibraryMetadataProviderConfigurationRepository(_nemeraDbContext);
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
            _artworkProviderConfigurationRepository ??= new ArtworkProviderConfigurationRepository(_nemeraDbContext);
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
            _libraryBookReaderConfigurationRepository ??= new LibraryBookReaderConfigurationRepository(_nemeraDbContext);
            return _libraryBookReaderConfigurationRepository;
        }
    }

    /// <summary>
    /// Gets the scheduled job repository.
    /// </summary>
    public IScheduledJobRepository ScheduledJobRepository
    {
        get
        {
            _scheduledJobRepository ??= new ScheduledJobRepository(_nemeraDbContext);
            return _scheduledJobRepository;
        }
    }

    /// <summary>
    /// Gets the scheduled job execution repository.
    /// </summary>
    public IScheduledJobExecutionRepository ScheduledJobExecutionRepository
    {
        get
        {
            _scheduledJobExecutionRepository ??= new ScheduledJobExecutionRepository(_nemeraDbContext);
            return _scheduledJobExecutionRepository;
        }
    }

    /// <summary>
    /// Gets the scheduler display preferences repository.
    /// </summary>
    public ISchedulerDisplayPreferencesRepository SchedulerDisplayPreferencesRepository
    {
        get
        {
            _schedulerDisplayPreferencesRepository ??= new SchedulerDisplayPreferencesRepository(_nemeraDbContext);
            return _schedulerDisplayPreferencesRepository;
        }
    }

    /// <summary>
    /// Gets the plugin repository.
    /// </summary>
    public IPluginRepository PluginRepository
    {
        get
        {
            _pluginRepository ??= new PluginRepository(_nemeraDbContext);
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
            _themeRepository ??= new ThemeRepository(_nemeraDbContext);
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
            _userRepository ??= new UserRepository(_nemeraDbContext);
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
            _userSettingsRepository ??= new UserSettingsRepository(_nemeraDbContext);
            return _userSettingsRepository;
        }
    }

    /// <summary>
    /// Gets the playlist repository.
    /// </summary>
    public IPlaylistRepository PlaylistRepository
    {
        get
        {
            _playlistRepository ??= new PlaylistRepository(_nemeraDbContext);
            return _playlistRepository;
        }
    }

    /// <summary>
    /// Gets the favorite repository.
    /// </summary>
    public IFavoriteRepository FavoriteRepository
    {
        get
        {
            _favoriteRepository ??= new FavoriteRepository(_nemeraDbContext);
            return _favoriteRepository;
        }
    }

    /// <summary>
    /// Gets the media consumption record repository.
    /// </summary>
    public IMediaConsumptionRecordRepository MediaConsumptionRecordRepository
    {
        get
        {
            _mediaConsumptionRecordRepository ??= new MediaConsumptionRecordRepository(_nemeraDbContext);
            return _mediaConsumptionRecordRepository;
        }
    }

    /// <summary>
    /// Gets the media resume point repository.
    /// </summary>
    public IMediaResumePointRepository MediaResumePointRepository
    {
        get
        {
            _mediaResumePointRepository ??= new MediaResumePointRepository(_nemeraDbContext);
            return _mediaResumePointRepository;
        }
    }

    /// <summary>
    /// Gets the repository for the consumption records used to build consumption statistics.
    /// </summary>
    public IConsumptionStatisticsRepository ConsumptionStatisticsRepository
    {
        get
        {
            _consumptionStatisticsRepository ??= new ConsumptionStatisticsRepository(_nemeraDbContext);
            return _consumptionStatisticsRepository;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="nemeraDbContext">Injected Entity Framework DbContext.</param>
    /// <param name="dateTimeProvider">Injected provider for the current UTC time, passed to the repositories that stamp their own timestamps.</param>
    public UnitOfWork(NemeraDbContext nemeraDbContext, IDateTimeProvider dateTimeProvider)
    {
        _nemeraDbContext = nemeraDbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Saves the changes made to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await SaveChangesCoreAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves the changes made during the one-time application initialization, whose audit values are provided explicitly by the bootstrap code instead of
    /// being stamped by the auditing interceptor.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> SaveChangesAsInitializationAsync(CancellationToken cancellationToken)
    {
        _nemeraDbContext.IsInitializationSave = true;
        try
        {
            return await SaveChangesCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _nemeraDbContext.IsInitializationSave = false;
        }
    }

    /// <summary>
    /// Saves the changes made to the database, reporting expected persistence conflicts as failure results.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> SaveChangesCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _nemeraDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success;
        }
        // A concurrency conflict means that another request changed the same row after this request read it. It is an expected persistence
        // conflict, so it is reported as a failure result instead of bubbling up as an unhandled exception.
        catch (DbUpdateConcurrencyException)
        {
            return Errors.Persistence.ConcurrencyConflict;
        }
        // A unique constraint violation is an expected persistence conflict, so it is reported as a failure result instead of bubbling up
        // as an unhandled exception and being turned into a generic internal server error.
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return Errors.Persistence.UniqueConstraintViolation;
        }
        // SQLite reports a busy or locked database when another writer holds the lock. It is a concurrent write conflict, not an unexpected
        // server failure, so it is reported the same way as an optimistic concurrency conflict.
        catch (DbUpdateException exception) when (IsSqliteConcurrencyConflict(exception))
        {
            return Errors.Persistence.ConcurrencyConflict;
        }
        catch (SqliteException exception) when (IsSqliteBusyOrLocked(exception))
        {
            return Errors.Persistence.ConcurrencyConflict;
        }
    }

    /// <summary>
    /// Determines whether the provided <paramref name="exception"/> was caused by a unique constraint violation of the storage medium.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><see langword="true"/> if the exception was caused by a unique constraint violation, <see langword="false"/> otherwise.</returns>
    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        // SQLite reports a generic constraint failure code for several constraints, so the extended code is used to tell a unique violation apart.
        return exception.InnerException is SqliteException sqliteException
            && sqliteException.SqliteExtendedErrorCode == 2067; // SQLITE_CONSTRAINT_UNIQUE.
    }

    /// <summary>
    /// Determines whether the provided <paramref name="exception"/> was caused by a SQLite write conflict.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><see langword="true"/> if the exception was caused by a busy or locked database, <see langword="false"/> otherwise.</returns>
    private static bool IsSqliteConcurrencyConflict(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqliteException && IsSqliteBusyOrLocked(sqliteException);
    }

    /// <summary>
    /// Determines whether the provided <paramref name="exception"/> reports a busy or locked SQLite database.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><see langword="true"/> if the database is busy or locked, <see langword="false"/> otherwise.</returns>
    private static bool IsSqliteBusyOrLocked(SqliteException exception)
    {
        return exception.SqliteErrorCode is 5 or 6; // SQLITE_BUSY, SQLITE_LOCKED.
    }

    /// <summary>
    /// Detaches all the entities tracked by the unit of work, freeing the memory they occupy.
    /// </summary>
    public void ClearTrackedEntities()
    {
        _nemeraDbContext.ChangeTracker.Clear();
    }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction = await _nemeraDbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
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
        _nemeraDbContext.Dispose();
    }
}
