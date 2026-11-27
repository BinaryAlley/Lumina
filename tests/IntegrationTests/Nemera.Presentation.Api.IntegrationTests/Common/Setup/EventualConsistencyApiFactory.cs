#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DomainEvents;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Authenticated API factory whose database transactions and domain events are observable through probes.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class EventualConsistencyApiFactory : AuthenticatedNemeraApiFactory
{
    /// <summary>
    /// Gets the probe that controls and observes the domain events queue.
    /// </summary>
    public DomainEventsQueueProbe QueueProbe { get; } = new();

    /// <summary>
    /// Gets the probe that observes the published domain events.
    /// </summary>
    public DomainEventPublicationProbe PublicationProbe { get; } = new();

    /// <summary>
    /// Gets the probe that observes the database transaction lifecycle.
    /// </summary>
    public TransactionProbe TransactionProbe { get; } = new();

    /// <summary>
    /// Clears every probe, so that a test observes only the activity started after the reset.
    /// </summary>
    public void ResetProbes()
    {
        QueueProbe.Reset();
        PublicationProbe.Reset();
        TransactionProbe.Reset();
    }

    /// <summary>
    /// Adds the transaction probe to the database context options.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the options of the database context.</param>
    /// <param name="options">The options builder of the database context being configured.</param>
    protected override void ConfigureDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        base.ConfigureDbContextOptions(serviceProvider, options);
        // The test services are registered before the shared factory configures the database, so the interceptor is available even from the
        // throwaway provider the shared factory uses to migrate the database, and a missing registration fails host construction loudly.
        options.AddInterceptors(serviceProvider.GetRequiredService<TransactionProbeInterceptor>());
    }

    /// <summary>
    /// Configures the web host, replacing the domain events queue and registering the recording event handler.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Registering the test services before the shared factory configures the database makes them available to its migration provider too.
        builder.ConfigureServices(services =>
        {
            services.AddHttpContextAccessor();
            services.AddSingleton(QueueProbe);
            services.AddSingleton(PublicationProbe);
            services.AddSingleton(TransactionProbe);
            services.AddSingleton<TransactionProbeInterceptor>();
            services.RemoveAll<IDomainEventsQueue>();
            services.AddScoped<IDomainEventsQueue, ProbingDomainEventsQueue>();
            services.AddScoped<IDomainEventHandler<FavoriteAddedDomainEvent>, RecordingFavoriteAddedDomainEventHandler>();
        });
        base.ConfigureWebHost(builder);
    }
}
