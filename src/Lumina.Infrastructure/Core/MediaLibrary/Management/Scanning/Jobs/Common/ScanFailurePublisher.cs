#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Helper for publishing the media library scan failure domain event, from within a media library scan job.
/// </summary>
internal static class ScanFailurePublisher
{
    /// <summary>
    /// Publishes the media library scan failure domain event for the scan that the job is part of.
    /// </summary>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which the publisher is resolved.</param>
    /// <param name="libraryId">The unique identifier of the library whose scan has failed.</param>
    /// <param name="compositeKey">Model for tracking media library scans.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public static async Task PublishAsync(IServiceScopeFactory serviceScopeFactory, LibraryId libraryId, MediaLibraryScanCompositeId compositeKey, Exception exception, CancellationToken cancellationToken)
    {
        // failure reporting is best-effort, a failure to report the failure must not crash the job processing
        try
        {
            await using AsyncServiceScope asyncServiceScope = serviceScopeFactory.CreateAsyncScope();
            IPublisher publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>()!;
            await publisher.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), libraryId, compositeKey, DateTime.UtcNow, exception.Message), cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // the failure reporting is best-effort, as the exception cannot be reported if the reporting itself fails
        }
    }
}
