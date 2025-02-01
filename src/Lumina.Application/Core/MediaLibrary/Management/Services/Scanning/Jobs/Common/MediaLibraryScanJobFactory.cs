#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;

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
    /// <typeparam name="TJob">The type of media library scan job to create.</typeparam>
    /// <returns>A media library scan job of type <typeparamref name="TJob"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested media library scan job has not been registered.</exception>
    public TJob CreateJob<TJob>(Library library) where TJob : MediaLibraryScanJob
    {
        // resolve the job from DI
        TJob job = _serviceProvider.GetRequiredService<TJob>() ?? throw new ArgumentException();
        job.Library = library;
        return job;
    }
}
