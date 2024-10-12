#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Models.FileManagement;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Mapster;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.FileManagement;

/// <summary>
/// Mapping configuration for file system directories between the domain object and the presentation object.
/// </summary>
public class DirectoryMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configuration.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Directory, FileSystemTreeNodeResponse>()
             .Map(dest => dest.Name, src => src.Name)
             .Map(dest => dest.Path, src => src.Id.Path)
             .Map(dest => dest.ItemType, src => src.Type)
             .Map(dest => dest.IsExpanded, () => false)
             .Map(dest => dest.ChildrenLoaded, () => false)
             .Map(dest => dest.Children, src => src.Items.Adapt<List<FileSystemTreeNodeResponse>>())
             .MapToConstructor(true);

        config.NewConfig<Directory, DirectoryResponse>()
            .MapWith(src => new DirectoryResponse(
                src.Id.Path,
                src.Name,
                src.DateCreated.Value,
                src.DateModified.Value,
                src.Items.Adapt<List<FileSystemItemModel>>()
            ));

        config.NewConfig<FileSystemItem, FileSystemItemModel>()
           .MapWith(src => new FileSystemItemModel(
               src.Id.Path,
               src.Name,
               DateTime.Now,
               DateTime.Now
           ));
    }
}
