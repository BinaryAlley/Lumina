#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Interface defining methods for handling file-related operations based on different sources.
/// </summary>
public interface IFileTypeService
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Determines if <paramref name="file"/> is of type image or not, and returns its type.
    /// </summary>
    /// <param name="file">The file to determine if it is an image or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the type of image or an error.</returns>
    Task<ErrorOr<ImageType>> GetImageTypeAsync(File file);

    /// <summary>
    /// Determines if a file identified by <paramref name="path"/> is of type image or not, and returns its type.
    /// </summary>
    /// <param name="path">The path of the file to determine if it is an image or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the type of image or an error.</returns>
    Task<ErrorOr<ImageType>> GetImageTypeAsync(FileSystemPathId path);
    #endregion
}