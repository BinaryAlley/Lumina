#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;

/// <summary>
/// Defines a factory for creating media library scanners.
/// </summary>
internal class MediaLibraryScannerFactory : IMediaLibraryScannerFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScannerFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public MediaLibraryScannerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a media library scanner for the specified library type.
    /// </summary>
    /// <param name="libraryType">The type of the media library.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a media library scanner for the specified media library type, or an error.
    /// </returns>
    public Result<IMediaTypeScanner> CreateLibraryScanner(LibraryType libraryType)
    {
        return libraryType switch
        {
            LibraryType.Book => Result.From<IMediaTypeScanner>(_serviceProvider.GetRequiredService<IBookLibraryTypeScanner>()),
            LibraryType.Music => Result.From<IMediaTypeScanner>(_serviceProvider.GetRequiredService<IMusicLibraryTypeScanner>()),
            // Every other media library type is modeled, but has no scanner yet, so scanning it is reported as unsupported.
            _ => Errors.LibraryScanning.NoScannerForLibraryType
        };
    }
}
