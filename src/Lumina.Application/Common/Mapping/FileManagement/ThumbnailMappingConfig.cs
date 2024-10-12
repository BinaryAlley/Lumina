#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Mapster;
#endregion

namespace Lumina.Application.Common.Mapping.FileManagement;

/// <summary>
/// Mapping configuration for file system thumbnails between the domain object and the presentation object.
/// </summary>
public class ThumbnailMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configuration.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Thumbnail, ThumbnailResponse>()
             .Map(dest => dest.Type, src => src.Type)
             .Map(dest => dest.Bytes, src => src.Bytes)
             .MapToConstructor(true);
    }
}
