#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Mapster;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileManagement;

/// <summary>
/// Mapping configuration for file system trees between the domain object and the presentation object.
/// </summary>
public class FileSystemTreeMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configuration.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<WindowsRootItem, FileSystemTreeNodeResponse>()
            .Map(dest => dest.Path, src => src.Id.Path)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ItemType, src => FileSystemItemType.Root)
            .Map(dest => dest.IsExpanded, () => false);

        config.NewConfig<FileSystemTreeNodeResponse, WindowsRootItem>()
        .MapWith(src => WindowsRootItem.Create(
            src.Path,
            src.Name,
            FileSystemItemStatus.Accessible)
        .Value);

        config.NewConfig<UnixRootItem, FileSystemTreeNodeResponse>()
            .Map(dest => dest.Path, src => src.Id.Path)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ItemType, src => FileSystemItemType.Root)
            .Map(dest => dest.IsExpanded, () => false)
            .Map(dest => dest.ChildrenLoaded, () => false)
            .Map(dest => dest.Children, src => src.Items.Select(item => item.Adapt<FileSystemTreeNodeResponse>(config)).ToList());

        config.NewConfig<FileSystemTreeNodeResponse, UnixRootItem>()
            .MapWith(src => UnixRootItem.Create(
                FileSystemItemStatus.Accessible)
            .Value);

        config.NewConfig<FileSystemItem, FileSystemTreeNodeResponse>()
             .Map(dest => dest.Name, src => src.Name)
             .Map(dest => dest.Path, src => src.Id.Path)
             .Map(dest => dest.ItemType, src => src.Type)
             .Map(dest => dest.IsExpanded, _ => false)
             .Map(dest => dest.ChildrenLoaded, _ => false)
             .Map(dest => dest.ChildrenLoaded, _ => false)
             .AfterMapping((src, dest) =>
             {
                 if (src is Directory dir)
                     dest.Children = dir.Items.Select(item => item.Adapt<FileSystemTreeNodeResponse>(config)).ToList();
                 else
                     dest.Children = [];
             });
    }
}
