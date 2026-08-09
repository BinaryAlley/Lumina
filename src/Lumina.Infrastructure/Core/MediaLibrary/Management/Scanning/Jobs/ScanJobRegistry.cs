#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Hooks;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs;

/// <summary>
/// Registry of the media library scan jobs that are injected by plugins at the defined hook points of the media library scan job graph.
/// </summary>
internal sealed class ScanJobRegistry : IScanJobRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<Type>> _jobsByHook;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanJobRegistry"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider, used to resolve the registered plugin jobs.</param>
    /// <param name="jobsByHook">The media library scan job types registered for each hook point, provided by the composition root of the plugin system.</param>
    public ScanJobRegistry(IServiceProvider serviceProvider, IReadOnlyDictionary<string, IReadOnlyList<Type>> jobsByHook)
    {
        _serviceProvider = serviceProvider;
        _jobsByHook = jobsByHook;
    }

    /// <summary>
    /// Gets the media library scan jobs registered for the provided <paramref name="hookName"/>.
    /// </summary>
    /// <param name="hookName">The name of the hook point at which the media library scan jobs were injected.</param>
    /// <param name="libraryId">The unique identifier of the media library upon which the scan is performed.</param>
    /// <returns>The collection of media library scan jobs registered at the provided hook point.</returns>
    public IEnumerable<IMediaLibraryScanJob> GetJobsForHook(string hookName, LibraryId libraryId)
    {
        // no plugin jobs are registered for this hook point, unless the plugin system provides them
        if (!_jobsByHook.TryGetValue(hookName, out IReadOnlyList<Type>? jobTypes))
            yield break;

        foreach (Type jobType in jobTypes)
        {
            IMediaLibraryScanJob pluginJob = (IMediaLibraryScanJob)_serviceProvider.GetRequiredService(jobType);
            pluginJob.LibraryId = libraryId;
            yield return pluginJob;
        }
    }
}
