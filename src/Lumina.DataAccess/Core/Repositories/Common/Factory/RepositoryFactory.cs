#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Common.Factory;

/// <summary>
/// Concrete implementation for the repositories factory.
/// </summary>
public class RepositoryFactory : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new repository of type <typeparamref name="TRepository"/>.
    /// </summary>
    /// <typeparam name="TRepository">The type of repository to create.</typeparam>
    /// <returns>A repository of type <typeparamref name="TRepository"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested repository has not been registered.</exception>
    public TRepository CreateRepository<TRepository>() where TRepository : notnull
    {
        return _serviceProvider.GetRequiredService<TRepository>() ?? throw new ArgumentException();
    }
}
