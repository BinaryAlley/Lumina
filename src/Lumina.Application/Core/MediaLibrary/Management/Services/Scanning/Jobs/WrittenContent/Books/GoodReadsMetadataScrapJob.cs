#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Media library scan job for retrieving written content metadata from GoodReads.
/// </summary>
internal sealed class GoodReadsMetadataScrapJob : MediaLibraryScanJob, IGoodReadsMetadataScrapJob
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
                Console.WriteLine("started goodreads metadata");
                List<int> ints = [1, 2, 3];
                Status = LibraryScanJobStatus.Completed;
                Console.WriteLine("ended goodreads metadata");
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
