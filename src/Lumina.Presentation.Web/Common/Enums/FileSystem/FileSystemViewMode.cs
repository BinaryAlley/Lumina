namespace Lumina.Presentation.Web.Common.Enums.FileSystem;

/// <summary>
/// Represents the different view modes for displaying file system items.
/// </summary>
public enum FileSystemViewMode
{
    /// <summary>
    /// Displays file system items in a detailed view, typically including additional information such as file size, date modified, and permissions.
    /// </summary>
    Details,

    /// <summary>
    /// Displays file system items in a compact list view, usually showing only the name of each item.
    /// </summary>
    List,

    /// <summary>
    /// Displays file system items as icons, providing a visual representation of each item type.
    /// </summary>
    Icons
}