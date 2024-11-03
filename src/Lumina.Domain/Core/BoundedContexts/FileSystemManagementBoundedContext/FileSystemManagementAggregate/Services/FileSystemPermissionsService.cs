#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Mono.Unix;
using Mono.Unix.Native;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Service for file system permissions.
/// </summary>
internal class FileSystemPermissionsService : IFileSystemPermissionsService // TODO: refactor towards abstractions that could allow testing
{
    private readonly IPlatformContextManager _platformContextManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemPermissionsService"/> class.
    /// </summary>
    /// <param name="platformContextManager">Injected facade service for platform contextual services.</param>
    public FileSystemPermissionsService(IPlatformContextManager platformContextManager)
    {
        _platformContextManager = platformContextManager;
    }

    /// <summary>
    /// Checks if <paramref name="path"/> can be accessed.
    /// </summary>
    /// <param name="path">The path to be accessed.</param>
    /// <param name="accessMode">The mode in which to access the path.</param>
    /// <param name="isFile">Indicates whether the path represents a file or directory.</param>
    /// <returns><see langword="true"/>, if <paramref name="path"/> can be accessed, <see langword="false"/> otherwise.</returns>
    public bool CanAccessPath(FileSystemPathId path, FileAccessMode accessMode, bool isFile = true)
    {
        PlatformType platformType = _platformContextManager.GetCurrentContext().Platform;
        if (platformType == PlatformType.Unix)
            return CanAccessPathLinux(path.Path, accessMode);
        else if (platformType == PlatformType.Windows)
#pragma warning disable CA1416 // Validate platform compatibility
            return CanAccessPathWindows(path.Path, accessMode, isFile);
#pragma warning restore CA1416 // Validate platform compatibility
        else
            throw new PlatformNotSupportedException("Support for this platform is not compiled into this assembly.");
    }

    /// <summary>
    /// Checks if a the current user has access permissions on Linux for <paramref name="path"/>, with <paramref name="accessMode"/>.
    /// </summary>
    /// <param name="path">The path for which to check the access.</param>
    /// <param name="accessMode">The access mode in which to check that path access.</param>
    /// <returns><see langword="true"/> if the current user has rights for the specified path and acccess mode, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when trying to check access for a mode that is not supported</exception>
    private static bool CanAccessPathLinux(string path, FileAccessMode accessMode)
    {
        AccessModes modes;
        UnixFileSystemInfo fileInfo = new UnixFileInfo(path);
        switch (accessMode)
        {
            case FileAccessMode.ReadProperties:
                // we would typically need execute permissions on a directory, to list its contents or view file properties
                if (fileInfo.FileType == FileTypes.Directory)
                    return fileInfo.CanAccess(AccessModes.X_OK);
                // for files, just verifying existence might be sufficient for reading properties
                return fileInfo.Exists;
            case FileAccessMode.ReadContents:
                return fileInfo.CanAccess(AccessModes.R_OK);
            case FileAccessMode.Write:
                modes = AccessModes.W_OK;
                break;
            case FileAccessMode.Execute:
                modes = AccessModes.X_OK;
                break;
            case FileAccessMode.ListDirectory:
                modes = AccessModes.F_OK;  // check existence, not an exact match but the closest
                break;
            case FileAccessMode.Delete:
                if (fileInfo.FileType == FileTypes.Directory)
                    return fileInfo.CanAccess(AccessModes.W_OK | AccessModes.X_OK); // to delete a directory, we need write and execute permissions on the directory itself
                else
                {
                    // to delete a file, we need write permission on the parent directory
                    string? parentDirectoryPath = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(parentDirectoryPath))
                        return false; // if path is a root directory or has no parent, deletion is not possible
                    UnixDirectoryInfo parentDirectoryInfo = new(parentDirectoryPath);
                    return parentDirectoryInfo.CanAccess(AccessModes.W_OK);
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(accessMode), "Unknown FileAccessMode");
        }
        return new UnixDirectoryInfo(path).CanAccess(modes);
    }

    /// <summary>
    /// Checks if a the current user has access permissions on Windows for <paramref name="path"/>, with <paramref name="accessMode"/>.
    /// </summary>
    /// <param name="path">The path for which to check the access.</param>
    /// <param name="accessMode">The access mode in which to check that path access.</param>
    /// <param name="isFile">Indicates whether the path represents a file or directory.</param>
    /// <returns><see langword="true"/> if the current user has rights for the specified path and acccess mode, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when trying to check access for a mode that is not supported.</exception>
    [SupportedOSPlatform("windows")]
    private static bool CanAccessPathWindows(string path, FileAccessMode accessMode, bool isFile = true)
    {
        // translate to filesystem access modes (multiple can match per case)
        FileSystemRights rights = accessMode switch
        {
            FileAccessMode.ReadProperties => FileSystemRights.ReadAttributes,
            FileAccessMode.ReadContents => FileSystemRights.ReadData | FileSystemRights.ReadExtendedAttributes,
            FileAccessMode.Write => FileSystemRights.WriteData | FileSystemRights.AppendData,
            FileAccessMode.Execute => FileSystemRights.ExecuteFile,
            FileAccessMode.ListDirectory => FileSystemRights.ListDirectory,
            FileAccessMode.Delete => FileSystemRights.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(accessMode), "Unknown FileAccessMode"),
        };
        if (rights == FileSystemRights.Delete)
        {
            // when checking for delete permissions, we need to check the directory the file or directory resides in
            string? parentDirectory = Directory.GetParent(path)?.FullName;
            if (string.IsNullOrEmpty(parentDirectory))
                return false; // the path is either a root directory or has no parent, which we cannot delete
            // checking for modify permission on the parent directory since delete requires modifying the parent contents
            return HasAccess(FileSystemRights.Modify, parentDirectory, false);
        }
        else
            if (HasAccess(rights, path, isFile))
            try
            {
                switch (accessMode)
                {
                    case FileAccessMode.ReadProperties:
                        // just accessing file info for properties, without opening it
                        FileSystemInfo fileSystemInfo = isFile ? new FileInfo(path) : new DirectoryInfo(path);
                        _ = fileSystemInfo.CreationTime;  // trigger potential access denial
                        break;
                    case FileAccessMode.ReadContents:
                        using (FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            stream.Close();
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        return false;
    }

    /// <summary>
    /// Checks if the current user or group has the access <paramref name="rights"/> for <paramref name="path"/>, on Windows platforms.
    /// </summary>
    /// <param name="rights">The rights for which to check access.</param>
    /// <param name="path">The path for which to check the access <paramref name="rights"/>.</param>
    /// <param name="isFile">Indicates whether the path represents a file or directory.</param>
    /// <returns><see langword="true"/>, if the current user or group has <paramref name="rights"/> for <paramref name="path"/>, <see langword="false"/> otherwise.</returns>
    [SupportedOSPlatform("windows")]
    private static bool HasAccess(FileSystemRights rights, string path, bool isFile = true)
    {
        bool allowAccess = false;
        AuthorizationRuleCollection acl;
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        // get the collection of authorization rules that apply to the specified directory
        try
        {
            // some paths (ex: C:\Windows\System32\config) throw "unauthorized" exceptions even when trying to check if one is authorized to access them - how dumb is that??...
            if (isFile)
            {
                FileInfo fileInfo = new(path);
                acl = fileInfo.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            else
            {
                DirectoryInfo directoryInfo = new(path);
                acl = directoryInfo.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            foreach (FileSystemAccessRule accessRule in acl)
                // Check if the current rule applies to the current user or the groups they belong to
                if (identity?.User?.Equals(accessRule.IdentityReference) == true || principal.IsInRole((SecurityIdentifier)accessRule.IdentityReference))
                    if (accessRule.AccessControlType.Equals(AccessControlType.Deny) && (accessRule.FileSystemRights & rights) == rights)
                        return false; // if there's a deny rule that matches the specified rights, return false immediately
                    else if (accessRule.AccessControlType.Equals(AccessControlType.Allow) && (accessRule.FileSystemRights & rights) == rights)
                        allowAccess = true;

        }
        catch
        {
            return false;
        }
        return allowAccess;
    }
}
