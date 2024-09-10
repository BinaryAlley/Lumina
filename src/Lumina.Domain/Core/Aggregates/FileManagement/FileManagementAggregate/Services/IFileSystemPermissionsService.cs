﻿#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Interface for file system permission service.
/// </summary>
public interface IFileSystemPermissionsService
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Checks if <paramref name="path"/> can be accessed.
    /// </summary>
    /// <param name="path">The path to be accessed.</param>
    /// <param name="accessMode">The mode in which to access the path.</param>
    /// <param name="isFile">Indicates whether the path represents a file or directory.</param>
    /// <returns><see langword="true"/>, if <paramref name="path"/> can be accessed, <see langword="false"/> otherwise.</returns>
    bool CanAccessPath(FileSystemPathId path, FileAccessMode accessMode, bool isFile = true);
    #endregion
}