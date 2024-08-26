#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.DependencyInjection;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Common.Factory;

/// <summary>
/// Concrete implementation for the repositories abstract factory.
/// </summary>
public class RepositoryFactory : IRepositoryFactory
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new repository of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of repository to create.</typeparam>
    /// <returns>A repository of type <typeparamref name="TResult"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested repository has not been registered.</exception>
    public TResult CreateRepository<TResult>() where TResult : notnull
    {
        return _serviceProvider.GetRequiredService<TResult>() ?? throw new ArgumentException();
    }
    #endregion
}