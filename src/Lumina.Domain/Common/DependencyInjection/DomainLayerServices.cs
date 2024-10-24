#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Strategies.Platform;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Domain.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Domain layer.
/// </summary>
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
        services.AddSingleton<IFileSystem, FileSystem>();
        return services;
    }
}
