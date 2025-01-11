#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Infrastructure.Models.Configuration;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Mediator;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the event raised when a media library is deleted.
/// </summary>
public class LibraryDeletedDomainEventHandler : INotificationHandler<LibraryDeletedDomainEvent>
{
    private readonly IEnvironmentContext _environmentContext;
    private readonly IPathService _pathService;
    private readonly MediaSettingsModel _mediaSettingsModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryDeletedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="mediaSettingsModelOptions">Injected service for retrieving <see cref="MediaSettingsModel"/>.</param>
    public LibraryDeletedDomainEventHandler(IEnvironmentContext environmentContext, IPathService pathService, IOptions<MediaSettingsModel> mediaSettingsModelOptions)
    {
        _environmentContext = environmentContext;
        _pathService = pathService;
        _mediaSettingsModel = mediaSettingsModelOptions.Value;
    }

    /// <summary>
    /// Handles the event raised when a media library is deleted.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public ValueTask Handle(LibraryDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // delete the media library directory in the internal location for media library files
        ErrorOr<string> libraryPathResult = GetLibraryPath(domainEvent.Library.Id.Value);
        if (libraryPathResult.IsError)
            throw new EventualConsistencyException(libraryPathResult.FirstError, libraryPathResult.Errors);

        ErrorOr<FileSystemPathId> newLibraryPathIdResult = FileSystemPathId.Create(libraryPathResult.Value);
        if (newLibraryPathIdResult.IsError)
            throw new EventualConsistencyException(newLibraryPathIdResult.FirstError, newLibraryPathIdResult.Errors);

        ErrorOr<Deleted> deleteLibraryDirectoryResult = _environmentContext.DirectoryProviderService.DeleteDirectory(newLibraryPathIdResult.Value);
        if (deleteLibraryDirectoryResult.IsError)
            throw new EventualConsistencyException(deleteLibraryDirectoryResult.FirstError, deleteLibraryDirectoryResult.Errors);
        
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the file system path of a media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The id of the media library for which to regtrieve the file system path.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either the file system path of the media library, or an error message.
    /// </returns>
    private ErrorOr<string> GetLibraryPath(Guid libraryId)
    {
        // root directory for media
        ErrorOr<string> rootPathResult = _pathService.CombinePath(AppContext.BaseDirectory, _mediaSettingsModel.RootDirectory);
        if (rootPathResult.IsError)
            return rootPathResult.Errors;

        // libraries directory
        ErrorOr<string> librariesPathResult = _pathService.CombinePath(rootPathResult.Value, _mediaSettingsModel.LibrariesDirectory);
        if (librariesPathResult.IsError)
            return librariesPathResult.Errors;

        // this new particular library' directory
        ErrorOr<string> libraryPathResult = _pathService.CombinePath(librariesPathResult.Value, libraryId.ToString());
        if (libraryPathResult.IsError)
            return libraryPathResult.Errors;
        return libraryPathResult.Value;
    }
}
