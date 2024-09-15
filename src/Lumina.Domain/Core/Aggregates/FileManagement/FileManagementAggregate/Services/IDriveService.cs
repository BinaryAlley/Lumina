#region ========================================================================= USING =====================================================================================
using ErrorOr;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Interface for the service for handling drives.
/// </summary>
public interface IDriveService
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Retrieves the list of drives.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of drives or an error.</returns>
    ErrorOr<IEnumerable<FileSystemItem>> GetDrives();
    #endregion
}