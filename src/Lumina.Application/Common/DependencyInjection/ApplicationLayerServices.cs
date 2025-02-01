#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Behaviors;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.WrittenContent.Books;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Queue;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.Common;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.WrittenContent;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Tracking;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Application layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApplicationLayerServices
{
    /// <summary>
    /// Registers the services of the Application layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        // register Mediator
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // register the validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // register fluent validators
        services.AddValidatorsFromAssembly(typeof(ApplicationLayerServices).Assembly);

        services.AddScoped<IDomainEventsQueue, DomainEventsQueue>();

        services.AddSingleton<IMediaLibrariesScanQueue, MediaLibrariesScanQueue>();
        services.AddSingleton<IMediaLibrariesScanTracker, MediaLibrariesScanTracker>();
        services.AddScoped<IBookLibraryTypeScanner, BookLibraryTypeScanner>();
        services.AddScoped<IMediaLibraryScanningService, MediaLibraryScanningService>();
        services.AddScoped<IMediaLibraryScannerFactory, MediaLibraryScannerFactory>();
        services.AddScoped<IMediaLibraryScanJobFactory, MediaLibraryScanJobFactory>();
        services.AddHostedService<MediaLibraryScanJobProcessorJob>();

        services.AddTransient<FileSystemDiscoveryJob>();
        services.AddTransient<RepositoryMetadataDiscoveryJob>();
        services.AddTransient<HashComparerJob>();
        services.AddTransient<GoodReadsMetadataScrapJob>();

        return services;
    }
}
