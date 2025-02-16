#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for comparing hashes.
/// </summary>
internal sealed class HashComparerJob : MediaLibraryScanJob, IHashComparerJob
{
    /// <inheritdoc/>
    public override async Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
    {
        try
        {
            parentsPayloadsExecuted++; // increment the number of parents that finished their execution and called this job
            // only execute this job's payload when it has no parents, or when all the parents finished their execution
            if (Parents.Count == 0 || parentsPayloadsExecuted == Parents.Count)
            {
                Status = LibraryScanJobStatus.Running;
                Console.WriteLine("started file hashing");
                List<int> ints = [1, 2, 3];
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1111, cancellationToken).ConfigureAwait(false);
                    Console.WriteLine("ticking");
                    cancellationToken.ThrowIfCancellationRequested();
                }
                Status = LibraryScanJobStatus.Completed;
                Console.WriteLine("ended file hashing");
                // call each linked child with the obtained payload
                foreach (IMediaLibraryScanJob children in Children)
                    await children.ExecuteAsync(id, ints, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
    }
}
