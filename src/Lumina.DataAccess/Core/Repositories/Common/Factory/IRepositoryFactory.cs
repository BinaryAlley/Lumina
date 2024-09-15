namespace Lumina.DataAccess.Core.Repositories.Common.Factory;

/// <summary>
/// Interface for the repositories factory.
/// </summary>
public interface IRepositoryFactory
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new repository of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of repository to create.</typeparam>
    /// <returns>A repository of type <typeparamref name="TResult"/>.</returns>
    TResult CreateRepository<TResult>() where TResult : notnull;
    #endregion
}