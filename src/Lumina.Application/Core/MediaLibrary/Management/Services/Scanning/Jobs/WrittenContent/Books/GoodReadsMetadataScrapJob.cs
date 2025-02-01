#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Media library scan job for retrieving written content metadata from GoodReads.
/// </summary>
internal class GoodReadsMetadataScrapJob : MediaLibraryScanJob
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
                List<int> ints = [1, 2, 3];
                Status = LibraryScanJobStatus.Completed;
                // call each linked child with the obtained payload
                foreach (MediaLibraryScanJob children in Children)
                    await children.ExecuteAsync(id, ints, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Cancelled;
            throw;
        }
    }
}
