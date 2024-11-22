namespace Lumina.DataAccess.Core.Repositories.Common.Factory;

/// <summary>
/// Interface for the repositories factory.
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>
    /// Creates a new repository of type <typeparamref name="TRepository"/>.
    /// </summary>
    /// <typeparam name="TRepository">The type of repository to create.</typeparam>
    /// <returns>A repository of type <typeparamref name="TRepository"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested repository has not been registered.</exception>
    TRepository CreateRepository<TRepository>() where TRepository : notnull;
}
