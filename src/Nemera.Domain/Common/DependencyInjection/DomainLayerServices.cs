#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.PathTemplate;
using Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Scanners;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Nemera.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.PathTemplate;
using Nemera.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
#endregion

namespace Nemera.Domain.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the Domain layer into the Dependency Injection container.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DomainLayerServices
{
    /// <summary>
    /// Registers the services of the Domain layer into the Dependency Injection container.
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
        services.AddSingleton<IPathService, PathService>();
        services.AddScoped<IFileTypeService, FileTypeService>();
        services.AddScoped<IEnvironmentContext, EnvironmentContext>();
        services.AddSingleton<IPlatformContextFactory, PlatformContextFactory>();
        services.AddSingleton<IUnixPlatformContext, UnixPlatformContext>();
        services.AddSingleton<IWindowsPlatformContext, WindowsPlatformContext>();
        services.AddSingleton<IPlatformContextManager, PlatformContextManager>();
        services.AddSingleton<IUnixPathStrategy, UnixPathStrategy>();
        services.AddSingleton<IWindowsPathStrategy, WindowsPathStrategy>();
        services.AddSingleton<IOperatingSystemInfo, OperatingSystemInfo>();
        services.AddScoped<IFileSystemStructureSeedService, FileSystemStructureSeedService>();
        services.AddScoped<IMediaLibraryScanJobFactory, MediaLibraryScanJobFactory>();
        services.AddScoped<IMediaLibraryScanningService, MediaLibraryScanningService>();
        services.AddScoped<IBookLibraryTypeScanner, BookLibraryTypeScanner>();
        services.AddScoped<IMusicLibraryTypeScanner, MusicLibraryTypeScanner>();
        services.AddScoped<IMediaLibraryScannerFactory, MediaLibraryScannerFactory>();
        services.AddScoped<ILibraryPathPartCatalog, MusicLibraryPathPartCatalog>();
        services.AddScoped<ILibraryPathPartCatalog, BookLibraryPathPartCatalog>();
        services.AddScoped<ILibraryPathTemplateService, LibraryPathTemplateService>();

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IMediaLibrariesScanProgressTracker, MediaLibrariesScanProgressTracker>();
        return services;
    }
}
