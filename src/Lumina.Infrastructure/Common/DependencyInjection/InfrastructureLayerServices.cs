#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Over18;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Application.Core.Scheduling.Notifications;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using Lumina.Infrastructure.Common.DomainEvents;
using Lumina.Infrastructure.Common.Models.DTO.Plugins;
using Lumina.Infrastructure.Core.Authentication;
using Lumina.Infrastructure.Core.Authorization;
using Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;
using Lumina.Infrastructure.Core.Authorization.Policies.LibraryOwnership;
using Lumina.Infrastructure.Core.Authorization.Policies.Over18;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Cancellation;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Progress;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Queue;
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Infrastructure.Core.Scheduling.Execution;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Lumina.Infrastructure.Core.Scheduling.Notifications;
using Lumina.Infrastructure.Core.Security;
using Lumina.Infrastructure.Core.Themes;
using Lumina.Infrastructure.Core.Time;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the Infrastructure layer into the Dependency Injection container.
/// </summary>
[ExcludeFromCodeCoverage]
public static class InfrastructureLayerServices
{
    /// <summary>
    /// Registers the services of the Infrastucture layer into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The configuration used to read the application configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Scan the current assembly for validators and add them to the DI container.
        IEnumerable<Type> concreteTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsInterface && !type.IsAbstract && !type.IsGenericTypeDefinition);

        foreach (Type implementation in concreteTypes)
            foreach (Type contract in implementation.GetInterfaces())
                if (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IValidator<>))
                    services.AddSingleton(contract, implementation);
      
        services.AddSingleton<IFileHashService, FileHashService>();
        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddSingleton<ICryptographyService, CryptographyService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IQRCodeGenerator, QRCodeGenerator>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<ITotpTokenGenerator, TotpTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IDomainEventsQueue, DomainEventsQueue>();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Authorization.
        services.AddScoped<IOver18Policy, Over18Policy>();
        services.AddScoped<ILibraryOwnershipPolicy, LibraryOwnershipPolicy>();
        services.AddScoped<IAuthorizationPolicyFactory, AuthorizationPolicyFactory>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        // Media library scanning.
        services.AddSingleton<IMediaLibrariesScanQueue, MediaLibrariesScanQueue>();
        services.AddSingleton<IMediaLibrariesScanCancellationTracker, MediaLibrariesScanCancellationTracker>();
        services.AddHostedService<MediaLibraryScanJobProcessorJob>();

        services.AddTransient<IBooksFileSystemDiscoveryJob, BooksFileSystemDiscoveryJob>();
        services.AddTransient<IMediaLibraryScanDiffJob, MediaLibraryScanDiffJob>();
        services.AddTransient<IMediaLibraryScanHashJob, MediaLibraryScanHashJob>();
        services.AddTransient<IMediaLibraryScanResultsSaveJob, MediaLibraryScanResultsSaveJob>();
        services.AddTransient<IMediaLibraryScanProviderConfigurationInvalidationJob, MediaLibraryScanProviderConfigurationInvalidationJob>();
        services.AddTransient<IMediaLibraryScanMetadataEnrichmentJob, MediaLibraryScanMetadataEnrichmentJob>();
        services.AddTransient<IMediaLibraryScanArtworkEnrichmentJob, MediaLibraryScanArtworkEnrichmentJob>();


        services.AddSingleton<IMediaLibraryScanProgressNotifier, DebouncedMediaLibraryScanProgressNotifier>();

        // Book artwork: stores the artwork of the books into the internal media directory.
        services.AddHttpClient();
        services.AddScoped<IBookArtworkService, BookArtworkService>();

        // Book reading: resolves the book reader plugins, extracts the books into a temporary directory, and serves their contents.
        // The enablement cache lets the reading service skip the per-request database read of the reader configurations.
        services.AddSingleton<IBookReaderEnablementCache, BookReaderEnablementCache>();
        services.AddSingleton<IBookReaderRegistry, BookReaderRegistry>();
        services.AddSingleton<IBookReadingService, BookReadingService>();

        // Plugins: load the plugin assemblies from the plugins directory, register their services and provide the plugin manager.
        string pluginsDirectorySetting = configuration.GetSection(PluginsSettingsDto.SECTION_NAME)["Directory"] ?? "plugins";
        string pluginsDirectory = Path.Combine(AppContext.BaseDirectory, pluginsDirectorySetting);
        PluginLoadResultDto pluginLoadResult = PluginLoader.LoadPlugins(pluginsDirectory, services);
        services.AddSingleton<IPluginManager>(new PluginManager(pluginLoadResult.Plugins, pluginLoadResult.LoadContexts));
        services.AddScoped<IPluginSettingsStore, PluginSettingsStore>();
        services.AddScoped<IPluginInstaller, PluginInstaller>();
        services.AddScoped<IMediaLibraryProviderConfigurationStore, MediaLibraryProviderConfigurationStore>();
        services.AddSingleton(serviceProvider => new PluginDetectionSyncJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            serviceProvider.GetRequiredService<IPluginManager>(),
            pluginLoadResult.Errors,
            serviceProvider.GetRequiredService<ILogger<PluginDetectionSyncJob>>()));
        services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<PluginDetectionSyncJob>());

        // Themes: the theme service stores and serves theme packs, and the detection job seeds the bundled themes at startup.
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddHostedService<ThemeDetectionSyncJob>();

        // Scheduled jobs: the scheduler executes the tasks of the scheduled jobs on their schedules, and the notifier broadcasts their state to the SignalR clients.
        services.AddSingleton<IScheduledJobScheduler, ScheduledJobSchedulerJob>();
        services.AddHostedService(serviceProvider => (ScheduledJobSchedulerJob)serviceProvider.GetRequiredService<IScheduledJobScheduler>());
        services.AddSingleton<IScheduledJobRuntimeRegistry, ScheduledJobRuntimeRegistry>();
        services.AddSingleton<IScheduledJobNotifier, ScheduledJobNotifier>();
        services.AddScoped<IScheduledTaskExecutorFactory, ScheduledTaskExecutorFactory>();
        services.AddScoped<MediaLibraryScanTaskExecutor>();
        services.AddScoped<TemporaryFilesCleanupTaskExecutor>();

        return services;
    }
}
