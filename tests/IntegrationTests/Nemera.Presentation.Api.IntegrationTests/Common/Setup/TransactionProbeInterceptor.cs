#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Database transaction interceptor that reports the rolled back transactions to a <see cref="TransactionProbe"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TransactionProbeInterceptor : DbTransactionInterceptor
{
    private readonly TransactionProbe _probe;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionProbeInterceptor"/> class.
    /// </summary>
    /// <param name="probe">The probe that records the rolled back transactions.</param>
    /// <param name="httpContextAccessor">Injected accessor used to identify the request whose transaction was rolled back.</param>
    public TransactionProbeInterceptor(TransactionProbe probe, IHttpContextAccessor httpContextAccessor)
    {
        _probe = probe;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Records that a transaction was rolled back.
    /// </summary>
    /// <param name="transaction">The rolled back transaction.</param>
    /// <param name="eventData">The data of the transaction event.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A completed task.</returns>
    public override Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        RecordRolledBack();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records that a transaction was rolled back synchronously.
    /// </summary>
    /// <param name="transaction">The rolled back transaction.</param>
    /// <param name="eventData">The data of the transaction event.</param>
    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        RecordRolledBack();
    }

    /// <summary>
    /// Reports the rolled back transaction of the current request to the probe.
    /// </summary>
    private void RecordRolledBack()
    {
        string path = _httpContextAccessor.HttpContext?.Request.Path.Value ?? string.Empty;
        _probe.RecordRolledBack(path);
    }
}
