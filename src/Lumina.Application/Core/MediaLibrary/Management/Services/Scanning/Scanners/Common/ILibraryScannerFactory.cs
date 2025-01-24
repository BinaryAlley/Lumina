#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.Common;

/// <summary>
/// Interface defining a factory for creating media library scanners.
/// </summary>
internal interface ILibraryScannerFactory
{
    /// <summary>
    /// Creates a media library scanner based on the provided library type.
    /// </summary>
    /// <param name="libraryType">The type of the library that determines the type of the created media library scanner.</param>
    /// <returns>A media library scanner for the specified media library type.</returns>
    IMediaTypeScanner CreateLibraryScanner(LibraryType libraryType);
}
