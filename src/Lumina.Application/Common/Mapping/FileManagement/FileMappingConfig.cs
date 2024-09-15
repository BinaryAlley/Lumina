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
    #region ===================================================================== METHODS ===================================================================================
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
    }
    #endregion
}