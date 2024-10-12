#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Mapster;
#endregion

namespace Lumina.Application.Common.Mapping.FileManagement;

/// <summary>
/// Mapping configuration for file system files between the domain object and the presentation object.
/// </summary>
public class FileMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configuration.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<File, FileSystemTreeNodeResponse>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Path, src => src.Id.Path)
            .Map(dest => dest.ItemType, src => src.Type)
            .Map(dest => dest.IsExpanded, () => false)
            .Map(dest => dest.ChildrenLoaded, () => false)
            .MapToConstructor(true);

        config.NewConfig<File, FileResponse>()
            .MapWith(src => new FileResponse(
                src.Id.Path,
                src.Name,
                src.DateCreated.HasValue ? src.DateCreated.Value : default,
                src.DateModified.HasValue ? src.DateModified.Value : default,
                src.Size
            ));
    }
}