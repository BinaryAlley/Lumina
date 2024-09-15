#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Mapster;
#endregion

namespace Lumina.Application.Common.Mapping.FileManagement;

/// <summary>
/// Mapping configuration for file system paths between the domain object and the presentation object.
/// </summary>
public class PathSegmentMappingConfig : IRegister
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Registers the mapping configuration.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PathSegment, PathSegmentResponse>()
            .Map(dest => dest.Path, src => src.Name)
            .MapToConstructor(true);
    }
    #endregion
}