#region ========================================================================= USING =====================================================================================
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.CQRS;

/// <summary>
/// Defines a contract for handling commands in the application layer.
/// Commands represent operations that modify application state.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the command.</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command and returns a result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the command execution.</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
