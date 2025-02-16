#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Concrete implementation for the media library scan jobs factory.
/// </summary>
internal class MediaLibraryScanJobFactory : IMediaLibraryScanJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanJobFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public MediaLibraryScanJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new media library scan job of type <typeparamref name="TJob"/>.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library upon which the scan is performed.</param>
    /// <typeparam name="TJob">The type of media library scan job to create.</typeparam>
    /// <returns>A media library scan job of type <typeparamref name="TJob"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested media library scan job has not been registered.</exception>
    public TJob CreateJob<TJob>(LibraryId libraryId) where TJob : IMediaLibraryScanJob
    {
        // resolve the job from DI
        TJob job = _serviceProvider.GetRequiredService<TJob>() ?? throw new ArgumentException("Job type not registered in the DI system.");
        job.LibraryId = libraryId;
        return job;
    }
}
