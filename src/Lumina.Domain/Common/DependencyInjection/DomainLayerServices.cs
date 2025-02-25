#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
#endregion

namespace Lumina.Domain.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Domain layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DomainLayerServices
{
    /// <summary>
    /// Registers the services of the Domain layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddDomainLayerServices(this IServiceCollection services)
    {
        services.AddScoped<IDriveService, DriveService>();
        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileSystemPermissionsService, FileSystemPermissionsService>();
        services.AddScoped<IDirectoryProviderService, DirectoryProviderService>();
        services.AddScoped<IFileProviderService, FileProviderService>();
        services.AddScoped<IThumbnailService, ThumbnailService>();
        services.AddScoped<IPathService, PathService>();
        services.AddScoped<IFileTypeService, FileTypeService>();
        services.AddScoped<IEnvironmentContext, EnvironmentContext>();
        services.AddScoped<IPlatformContextFactory, PlatformContextFactory>();
        services.AddScoped<IUnixPlatformContext, UnixPlatformContext>();
        services.AddScoped<IWindowsPlatformContext, WindowsPlatformContext>();
        services.AddScoped<IPlatformContextManager, PlatformContextManager>();
        services.AddScoped<IUnixPathStrategy, UnixPathStrategy>();
        services.AddScoped<IWindowsPathStrategy, WindowsPathStrategy>();
        services.AddScoped<IOperatingSystemInfo, OperatingSystemInfo>();
        services.AddScoped<IFileSystemStructureSeedService, FileSystemStructureSeedService>();
        services.AddScoped<IMediaLibraryScanJobFactory, MediaLibraryScanJobFactory>();
        services.AddScoped<IMediaLibraryScanningService, MediaLibraryScanningService>();
        services.AddScoped<IBookLibraryTypeScanner, BookLibraryTypeScanner>();
        services.AddScoped<IMediaLibraryScannerFactory, MediaLibraryScannerFactory>();

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IMediaLibrariesScanProgressTracker, MediaLibrariesScanProgressTracker>();
        return services;
    }
}
