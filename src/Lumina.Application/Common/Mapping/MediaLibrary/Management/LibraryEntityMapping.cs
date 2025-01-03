#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="LibraryEntity"/>.
/// </summary>
public static class LibraryEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="LibraryResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static LibraryResponse ToResponse(this LibraryEntity repositoryEntity)
    {
        return new LibraryResponse(
            repositoryEntity.Id,
            repositoryEntity.UserId,
            repositoryEntity.Title,
            repositoryEntity.LibraryType,
            repositoryEntity.ContentLocations.Select(location => location.Path).ToList(),
            repositoryEntity.CoverImage,
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc
        );
    }
}
