#region ========================================================================= USING =====================================================================================
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.UoW;

/// <summary>
/// Interaction boundary with the Data Access Layer.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Exposes a repository of type <typeparamref name="TRepository"/> to the Business Layer.
    /// </summary>
    /// <typeparam name="TRepository">The type of the exposed repository.</typeparam>
    /// <returns>A repository of type <typeparamref name="TRepository"/>.</returns>
    TRepository GetRepository<TRepository>();

    /// <summary>
    /// Saves all changes made to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Opens a transaction.
    /// </summary>
    void OpenTransaction();

    /// <summary>
    /// Commits a transaction.
    /// </summary>
    void CommitTransaction();
}