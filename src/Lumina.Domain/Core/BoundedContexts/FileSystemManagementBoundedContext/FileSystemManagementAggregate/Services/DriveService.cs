#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Service for handling drives.
/// </summary>
public class DriveService : IDriveService
{
    private readonly IFileSystem _fileSystem;
    private readonly IPlatformContext _platformContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DriveService"/> class.
    /// </summary>
    /// <param name="fileSystem">Injected service used to interact with the local filesystem.</param>
    public DriveService(IFileSystem fileSystem, IPlatformContextManager platformContextManager)
    {
        _fileSystem = fileSystem;
        _platformContext = platformContextManager.GetCurrentContext();
    }

    /// <summary>
    /// Retrieves the list of drives.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of drives or an error.</returns>
    public ErrorOr<IEnumerable<FileSystemItem>> GetDrives()
    {
        if (_platformContext.Platform == PlatformType.Unix)
        {
            ErrorOr<UnixRootItem> unixRootResult = UnixRootItem.Create(FileSystemItemStatus.Accessible);
            if (unixRootResult.IsError)
                return unixRootResult.Errors;
            return new List<FileSystemItem>() { unixRootResult.Value };
        }
        else
            return ErrorOrFactory.From(_fileSystem.DriveInfo.GetDrives()
                                                            .OrderBy(driveInfo => driveInfo.Name)
                                                            .Where(driveInfo => driveInfo.IsReady)
                                                            .Select(driveInfo =>
                                                            {
                                                                ErrorOr<FileSystemItem> root;
                                                                ErrorOr<WindowsRootItem> windowsRootResult = WindowsRootItem.Create(driveInfo.Name, driveInfo.Name);
                                                                if (windowsRootResult.IsError)
                                                                    return windowsRootResult.Errors;
                                                                else
                                                                    root = windowsRootResult.Value;
                                                                return root;
                                                            })
                                                            .Where(errorOrDrive => !errorOrDrive.IsError)
                                                            .Select(errorOrDrive => errorOrDrive.Value)
                                                            .AsEnumerable());
    }
}
